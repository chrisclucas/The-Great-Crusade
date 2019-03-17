//#define RESTORE_IN_RAM

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
// Communications:
using System.Net;
using System.Net.Sockets;
using System.Threading;

/*
 * File Transferer / File Server V1.2:
 * 
 * Transfer big files locally between devices through UDP.
 * You can send files of any size, the only limitation is the
 * available storage on destination device.
 * FileTransferServer uses a protocol for transferences.
 * 
 * This asset was downloaded from the Unity AssetStore:
 * https://www.assetstore.unity3d.com/#!/content/73518
 * 
 * V 1.1 Features:
 * - Transfer speed increased 400%.
 * 
 * V 1.2 Features:
 * - New connection error event (IP/PORT pair duplicated for UDP).
 * - Performance significantly improved for multiple simultaneus downloads.
 * - New feature: Server can "Send" a file.
 * - New feature: Server can "Send" an update.
 * - New feature: Files can include a path in server side.
 * - New feature: Files and updates can be saved into a custom path.
 * - Includes FileManagement V1.3
 */

// Relevant information to request a file:
struct FileRequest
{
    public string file;
    public string serverIP;
    public string savePath;
    public bool fullPath;
}

public class FileTransferServer : MonoBehaviour
{

    // UDP communication elements:
    [SerializeField]
    //int port = 2600;                           // File transfering port (set your own if you wish).
    Thread receiveThread;                                       // Thread listening incoming file requests.
    UdpClient client;                                           // Communications object.
    IPEndPoint remoteEndPoint;                                  // Target IP or URL, to where a package is sent.
    IPEndPoint anyIP;                                           // Local source IP, listening any IP without restrictions.
    List<string> messageBuffer = new List<string>();            // The incoming messages.
    List<string> validServers = new List<string>();             // List of detected servers.

    // Transfer protocol configurations:
    [SerializeField]
    string tempFolder = "FTS_TempDownload";    // Temporary download folder (to keep things clean).
    [SerializeField]
    bool enableServer = true;
    [SerializeField]
    int maxChunkSize = 65536;                  // Maximum size of partial files.
    [SerializeField]
    float statusTimeout = 3f;                  // Timeout when waiting answer from remote file server status.
    [SerializeField]
    float rxFileTimeout = 0.5f;                // Timeout when waiting for a file/partialFile from remote server.
    [SerializeField]
    int rxFileRetryMaxCnt = 5;                 // Maximum of failed "rxFileTimeout" allowed before fire event.
    [SerializeField]
    float rxListTimeout = 1f;                  // Timeout when waiting for update list from server.

    // Internal status:
    bool waitForRemoteStatus = false;                           // Wait for remote server status.
    float remoteStatusTimer = 0f;                               // Timeout for remote server status.
    string[] partialFileList = { };                             // List of partial files.
    float rxFileTimer = 0f;                                     // Timeout for file request.
    int rxFileRetryCounter = 0;                                 // Counts the "rxFileTimer" loops while waiting for a file request.
    List<FileRequest> downloadList = new List<FileRequest>();   // List of batched files to download.
    bool waitForUpdateList = false;                             // Wait for update list.
    float updateListTimer = 0f;                                 // Timeout for update list.
    string updateSavePath = "";                                 // The requested path where all downloaded files will be saved.
    bool updateFullPath = false;                                // The nature of the updateSavePath, full or relative to PersistentData.

    // Events:
    public UnityEvent onStatusReponse;      // The remote server has sent a response to F0 message.
    public UnityEvent onStatusTimeout;      // The remote server never answered to F0 message.
    public UnityEvent onFileDownload;       // A file was downloaded properly.
    public UnityEvent onFileNotFound;       // Server doesn't has the requested file.
    public UnityEvent onFileTimeout;        // Server is not responding to file request after retrying "rxFileRetryMaxCnt" times.
    public UnityEvent onListDownload;       // The download list was finished.
    public UnityEvent onListNotFound;       // Server doesn't has the requested update list.
    public UnityEvent onUpdateTimeout;      // Update list request has no response.
    public UnityEvent onConnectionFailed;   // When a UDP IP/PORT pair is duplicated and can't be used. "FTS"

    /*********************
     * UDP Communications:
     *********************/
    /// <summary>Creates UDP Client and starts the listening thread</summary>
    //void Awake()
    public void initiateFileTransferServer()
    {
        // UDP client:
        if (client == null)
        {
            try
            {
                //client = new UdpClient(port);
                client = new UdpClient(TransportScript.remoteGamePort);
                client.EnableBroadcast = true;
                client.Client.ReceiveBufferSize = 65536;	// Forces the highest value (64KB).
                client.Client.SendBufferSize = 65536;		// Forces the highest value (64KB).
                SetMaxChunkSize(maxChunkSize);              // Sets the maximum available.
            }
            catch (System.Exception e)
            {
                GlobalDefinitions.WriteToLogFile("[FIleTransferServer.Awake] UDP ERROR: " + e.Message);
                if (onConnectionFailed != null)
                    onConnectionFailed.Invoke();
            }
        }
        // Thread listening incoming messages:
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        // Create the temporary download folder if not exists:
        if (!FileManagement.DirectoryExists(tempFolder))
            FileManagement.CreateDirectory(tempFolder);
        // Delete last possible incomplete previous download:
        FileManagement.EmptyDirectory(tempFolder);
    }
    /// <summary>Listening thread</summary>
    void ReceiveData()
    {
        while (client != null)
        {
            try
            {
                // Starts listening any IP:
                if (anyIP == null)
                    anyIP = new IPEndPoint(IPAddress.Any, 0);
                // Reads received data:
                byte[] data = client.Receive(ref anyIP);
                char[] chars = new char[data.Length];
                for (int c = 0; c < data.Length; c++)
                    chars[c] = (char)data[c];
                string message = new string(chars);
                // Basic message integrity verification:
                if (message.Length > 0 && message[message.Length - 1] == '#')
                    messageBuffer.Add(message);     // Add received message to buffer.
            }
            catch { }
        }
    }
    /// <summary>Analyze incoming messages (this analysis is not running inside the listening thread)</summary>
    void MessageAnalysis(string message)
    {
        /* Possible messages:
         * 
         * F0;ClientIP;#                                        : Server polling request.
         * F1;ServerIP;enabled;#                                : Server polling response.
         * 
         * F2;ServerIP;FileName;#                               : Requested file not found.
         * F3;ClientIP;File.ext;part;#                          : Request a partial file.
         * F4;ServerIP;File.ext;part,total;BinaryFileContent;#  : A partial file.
         * 
         * F5;ServerIP;list;#                                   : Requested "file update" doesn't exists.
         * F6;ClientIP;list;#                                   : Request the "file update" list with custom name.
         * F7;ServerIP;File01.ext;File02.ext;FileNN.ext;#       : The "file update" list.
         * 
         * F8;ServerIP;name;#                                   : The Server requests a transference.
         * F9;ServerIP;list;#                                   : The Server requests an update.
         */

        string[] fields = message.Split(';');   // Retrieves message fields.

        // Echo filter:
        string remoteIp = fields[1];
        if (GlobalDefinitions.thisComputerIPAddress != GlobalDefinitions.opponentIPAddress)
        {
            switch (fields[0])
            {
                case "F0":  // Someone wants to know if this server is active.
                    Thread.Sleep(Random.Range(200, 800));
                    SendString(remoteIp, "F1;" + GlobalDefinitions.thisComputerIPAddress + ";" + enableServer.ToString() + ";#");
                    break;
                case "F1":  // Server polling response (can be disabled):
                    bool activity = bool.Parse(fields[2]);
                    if (activity)
                        AddValidServer(fields[1]);
                    else
                        RemoveValidServer(fields[1]);
                    if (onStatusReponse != null)
                        onStatusReponse.Invoke();
                    // Deactivate timeout timer:
                    waitForRemoteStatus = false;
                    remoteStatusTimer = 0f;
                    break;
                case "F2":  // The requested file doesn't exists.
                    if (onFileNotFound != null)
                        onFileNotFound.Invoke();
                    // Remove from the "download list":
                    RemoveFileFromDownload(fields[1], fields[2]);
                    rxFileTimer = 0f;
                    break;
                case "F3":  // Send a partial file message.
                    if (enableServer)
                        SendPartialFile(fields[1], fields[2], fields[3]);
                    break;
                case "F4":  // Receiving a requested partial file.
                    //
                    if (downloadList.Count > 0 && fields[1] == GlobalDefinitions.opponentIPAddress && fields[2] == downloadList[0].file)
                    {
                        rxFileRetryCounter = 0;                 // Reset the "file request" retry counter.
                        string part = fields[3].Split(',')[0];  // Partial file order.
                        string total = fields[3].Split(',')[1]; // Total file parts.
                        if (partialFileList.Length == 0)        // File download has started.
                            partialFileList = new string[int.Parse(total)];             // Create file parts lists if doesn't exists.
                        // File content:
                        int cnt = 0;
                        int i = 0;
                        for (i = 0; i < message.Length; i++)
                        {
                            if (message[i] == ';')
                                cnt++;
                            if (cnt == 4)
                                break;    // Count 4 fields until file content (to avoid data loss).
                        }
                        string fileContent = message.Substring(i + 1, message.Length - i - 3);
                        // Save partial file, the file is saved temporarily with its part number:
                        string fileName = FileManagement.GetFileName(fields[2]);
                        FileManagement.SaveFile(FileManagement.Combine(tempFolder, fileName + part), fileContent);
                        // Add partial file to the downloaded list:
                        partialFileList[int.Parse(part) - 1] = fileName + part; // This names are needed to restore the original file.
                        rxFileTimer = 0f;
                        // Restore the original file:
                        int completeCnt = 0;
                        for (int c = 0; c < partialFileList.Length; c++)
                        {
                            if (partialFileList[c] != null && partialFileList[c] != "")
                                completeCnt++;
                        }
                        // Is it complete
                        if (completeCnt == partialFileList.Length)
                        {
                            RestorePartialFile(fileName, downloadList[0].savePath, downloadList[0].fullPath);
                            // Erase temporary parts list:
                            partialFileList = new string[0];
                            FileManagement.EmptyDirectory(tempFolder);
                            // File download event:
                            if (onFileDownload != null)
                                onFileDownload.Invoke();

                            // Remove from the "download list":
                            RemoveFileFromDownload(fields[1], fields[2]);

                            TransportScript.SendMessageToRemoteComputer(GlobalDefinitions.GAMEDATALOADEDKEYWORD);
                            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().ReadTurnFile(fileName);
                        }
                    }
                    break;
                case "F5":  // Requested "file update" list doesn't exists.
                    waitForUpdateList = false;
                    updateListTimer = 0f;
                    // List not found event:
                    if (onListNotFound != null)
                        onListNotFound.Invoke();
                    break;
                case "F6":  // Sending the local "file update" list.
                    if (enableServer)
                        SendUpdateList(fields[1], fields[2]);
                    break;
                case "F7":  // Receiving the "file update" list.
                    waitForUpdateList = false;
                    updateListTimer = 0f;
                    // Load the list into the download queue:
                    message = message.Substring(3, message.Length - 5);
                    string[] fileList = message.Split(';');
                    for (int l = 1; l < fileList.Length; l++)
                    {
                        AddFileToDownload(fileList[0], fileList[l], updateSavePath, updateFullPath);
                    }
                    break;
                case "F8":  // Server requested a transference.
                    AddValidServer(fields[1]);
                    AddFileToDownload(fields[1], fields[2]);
                    break;
                case "F9":  // Server requested an update.
                    AddValidServer(fields[1]);
                    RequestUpdateList(fields[1], fields[2]);
                    break;
                default:
                    break;
            }   // Switch.
        }   // Echo filter.
    }
    /// <summary>Send a string message through UDP</summary>
    void SendString(string ip, string msg)
    {
        byte[] data = new byte[msg.Length];
        for (int c = 0; c < msg.Length; c++)
            data[c] = (byte)msg[c];    // Safe convertion from string to byte[]
        SendData(ip, data);
    }
    /// <summary>Send byte[] message through UDP.</summary>
    void SendData(string ip, byte[] data)
    {
        if (client != null)
        {
            // Create the remote connection address:
            IPAddress ipAddress = IpParse(ip);
            if (ipAddress == null)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);          // Gets the IP from a URL
                ipAddress = ipHostInfo.AddressList[0];
                //remoteEndPoint = new IPEndPoint(ipAddress, port);       // Remote point generated from URL.
                remoteEndPoint = new IPEndPoint(ipAddress, TransportScript.remoteGamePort);
            }
            else
            {
                //remoteEndPoint = new IPEndPoint(ipAddress, port);       // Remote point generated from IP.
                remoteEndPoint = new IPEndPoint(ipAddress, TransportScript.remoteGamePort);
            }
            // If the message is too big will fail:
            if (data.Length > client.Client.SendBufferSize)
            {
                GlobalDefinitions.WriteToLogFile("ERROR - SendData: Message bigger than " + client.Client.SendBufferSize.ToString() + " bytes");
            }
            else
            {
                client.Send(data, data.Length, remoteEndPoint);
            }
        }
    }
    /// <summary>String IP parser. If not parsed correctly returns null without crashing.</summary>
    static IPAddress IpParse(string ipAddress)
    {
        IPAddress address = null;
        if (ipAddress != "")
        {
            try
            {
                // Create an instance of IPAddress for IP V4 or V6:
                address = IPAddress.Parse(ipAddress);
                return address;
            }
            catch { }
        }
        return address;
    }
    /// <summary>Timers and timeout controls</summary>
    void Update()
    {
        // Messages can't be analyzed into the receive thread or Unity may crash:
        if (messageBuffer.Count > 0)
        {
            // Analyze incoming message:
            string message = messageBuffer[0];
            messageBuffer.RemoveAt(0);
            MessageAnalysis(message);
        }

        // The File Update list starts the single file download process:
        if (downloadList.Count > 0)
        {
            // Retry timeout:
            rxFileTimer -= Time.deltaTime;
            if (rxFileTimer <= 0f)
            {
                // If there are files to be downloaded but none is started:
                if (partialFileList.Length == 0)
                {
                    // Request first part of the file (or retry):
                    rxFileTimer = rxFileTimeout;    // Reset file request timer.
                    FileRequest item = downloadList[0];
                    GlobalDefinitions.WriteToLogFile("FileTransferServer update(): SendString(" + GlobalDefinitions.opponentIPAddress + " F3;" + GlobalDefinitions.thisComputerIPAddress + ";" + item.file + ";1;#");
                    SendString(GlobalDefinitions.opponentIPAddress, "F3;" + GlobalDefinitions.thisComputerIPAddress + ";" + item.file + ";1;#");
                }
                else
                {
                    // We are downloading some file:
                    for (int p = 0; p < partialFileList.Length; p++)
                    {
                        if (partialFileList[p] == null)
                        {
                            // Request next part of the file (or retry):
                            rxFileTimer = rxFileTimeout;    // Reset file request timer.
                            FileRequest item = downloadList[0];
                            SendString(GlobalDefinitions.opponentIPAddress, "F3;" + GlobalDefinitions.thisComputerIPAddress + ";" + item.file + ";" + (p + 1).ToString() + ";#");
                            // Counter of retry attempts, fire event if maximum reached:
                            rxFileRetryCounter++;
                            if (rxFileRetryCounter == rxFileRetryMaxCnt && onFileTimeout != null)
                            {
                                rxFileRetryCounter = 0;
                                onFileTimeout.Invoke();
                            }
                            break;
                        }
                    }
                }
            }
        }

        // Waiting for a remote server status:
        if (waitForRemoteStatus)
        {
            remoteStatusTimer -= Time.deltaTime;
            if (remoteStatusTimer <= 0f)
            {
                // F1 Timeout event:
                remoteStatusTimer = 0f;
                waitForRemoteStatus = false;
                if (onStatusTimeout != null)
                    onStatusTimeout.Invoke();
            }
        }

        // Waiting for an update list:
        if (waitForUpdateList)
        {
            updateListTimer -= Time.deltaTime;
            if (updateListTimer <= 0f)
            {
                // F7 Timeout event:
                updateListTimer = rxListTimeout;
                waitForUpdateList = false;
                if (onUpdateTimeout != null)
                    onUpdateTimeout.Invoke();
            }
        }
    }
    /// <summary>Disconnects UDP port on closing, scene switching and object destroy</summary>
    void Disconnect()
    {
        if (receiveThread != null)
            receiveThread.Abort();
        if (client != null)
            client.Close();
    }
    void OnApplicationQuit()
    {
        Disconnect();
    }
    void OnDestroy()
    {
        Disconnect();
    }

    /********************
     * Client interfaces:
     ********************/
    /// <summary>Check server status</summary>
    public void CheckServerStatus(string serverIP)
    {
        // Load the timeout timer:
        waitForRemoteStatus = true;
        remoteStatusTimer = statusTimeout;
        // Send the response:
        SendString(serverIP, "F0;" + GlobalDefinitions.thisComputerIPAddress + ";#");
    }
    /// <summary>Valid server list control</summary>
    public List<string> GetServerList()
    {
        return validServers;
    }
    public void ResetServerList()
    {
        validServers.Clear();
    }
    void AddValidServer(string ip)
    {
        foreach (string serverIP in validServers)
        {
            if (serverIP == ip) return;     // This IP already exists, will not add again.
        }
        // Add the new IP:
        validServers.Add(ip);
    }
    void RemoveValidServer(string ip)
    {
        foreach (string serverIP in validServers)
        {
            if (serverIP == ip)
            {
                validServers.Remove(serverIP);
                return;
            }
        }
    }
    /// <summary>Request a file by Server IP or Server index</summary>
    public void RequestFile(string serverIP, string file, string savePath = "", bool fullPath = false)
    {
        AddFileToDownload(serverIP, file, savePath, fullPath);
    }
    public void RequestFile(int serverIndex, string file, string savePath = "", bool fullPath = false)
    {
        if (validServers.Count > 0)
            RequestFile(validServers[serverIndex], file, savePath, fullPath);
    }
    /// <summary>Batch list control</summary>
    void AddFileToDownload(string serverIP, string file, string savePath = "", bool fullPath = false)
    {
        FileRequest item = new FileRequest();
        item.file = file;
        item.serverIP = serverIP;
        item.savePath = savePath;
        item.fullPath = fullPath;
        // Avoid repeating items:
        if (downloadList.Contains(item))
            return;
        else
        {
            downloadList.Add(item);
        }
    }
    void RemoveFileFromDownload(string serverIP, string file)
    {
        FileRequest deleteItem = new FileRequest();
        foreach (FileRequest item in downloadList)
        {
            if (item.file == file)
            {
                deleteItem = item;
                //break;
            }
        }
        
        downloadList.Remove(deleteItem);

        // Download list complete event:
        if (downloadList.Count == 0 && onListDownload != null)
            onListDownload.Invoke();
    }
    /// <summary>Request the "file update" batch list by IP or Server index</summary>
    public void RequestUpdateList(string serverIP, string file, string savePath = "", bool fullPath = false)
    {
        SendString(serverIP, "F6;" + GlobalDefinitions.thisComputerIPAddress + ";" + file + ";#");
        waitForUpdateList = true;
        updateListTimer = rxListTimeout;
        updateSavePath = savePath;
        updateFullPath = fullPath;
    }
    public void RequestUpdateList(int serverIndex, string file, string savePath = "", bool fullPath = false)
    {
        if (validServers.Count > 0)
            RequestUpdateList(validServers[serverIndex], file, savePath, fullPath);
    }
    /// <summary>Restores the original file from temporary parts (to disk)</summary>
    void RestorePartialFile(string name, string savePath, bool fullPath)
    {
        // Add the requested destination folder:
        name = FileManagement.Combine(savePath, name);
        // If already exists, is deleted:
        if (FileManagement.FileExists(name, false, fullPath))
            FileManagement.DeleteFile(name, fullPath);
#if RESTORE_IN_RAM
        // Add all temporary parts in memory, then saves to disk:
        List<byte> content = new List<byte>();
        for (int c = 0; c < partialFileList.Length; c++)
        {
            string fileName = FileManagement.Combine(tempFolder, FileManagement.GetFileName(partialFileList[c]));
            content.AddRange(FileManagement.ReadRawFile(fileName));
            FileManagement.DeleteFile(fileName);  // Delete the already added part.
        }
        FileManagement.SaveRawFile(name, content.ToArray(), false, fullPath);
#else
        // Creates a stream to restore the original file (not compatible with Windows Phone):
        if (!fullPath)
            name = FileManagement.Combine(Application.persistentDataPath, name);
        System.IO.FileStream file = System.IO.File.Open(name, System.IO.FileMode.Append, System.IO.FileAccess.Write);
        // Add all temporary parts:
        for (int c = 0; c < partialFileList.Length; c++)
        {
            string fileName = FileManagement.Combine(tempFolder, FileManagement.GetFileName(partialFileList[c]));
            byte[] filePart = FileManagement.ReadRawFile(fileName);
            file.Write(filePart, 0, filePart.Length);
            FileManagement.DeleteFile(fileName);  // Delete the already added part.
        }
        file.Close();
#endif        
    }
    /// <summary>Aborts the file download in progress</summary>
    public void AbortDownloadInProgress()
    {
        if (downloadList.Count > 0)
        {
            // Remove file from update list:
            FileRequest fr = downloadList[0];
            RemoveFileFromDownload(fr.file, fr.serverIP);
            // Reset download list.
            partialFileList = new string[0];
        }
        // Delete temporary disk files:
        FileManagement.EmptyDirectory(tempFolder);
        // Reset timers.
        rxFileRetryCounter = 0;
        rxFileTimer = 0;
    }
    /// <summary>Aborts all downloads in the list</summary>
    public void AbortDownloadList()
    {
        AbortDownloadInProgress();
        downloadList.Clear();
    }

    /********************
     * Server interfaces:
     ********************/
    /// <summary>Set server capabilities:
    public void SetLocalServerStatus(bool enabled)
    {
        // If disabled can download but not upload.
        enableServer = enabled;
    }
    /// <summary>Set chunk size (300 to 65336)</summary>
    public void SetMaxChunkSize(int chunk)
    {
        // Hardware limitation:
        maxChunkSize = client.Client.SendBufferSize - 300;  // Gets the assigned value (may be less than the forced one).
        // Requested value:
        if (chunk < maxChunkSize)
            maxChunkSize = chunk;
        // Clamp:
        if (maxChunkSize < 300)
            maxChunkSize = 300;
    }
    /// <summary>Get chunk size</summary>
    public int GetMaxChunkSize()
    {
        return maxChunkSize;
    }
    /// <summary>Returns the requested part of a file (1 to n) from disk</summary>
    byte[] ReadPartialFile(string name, int part = 1)
    {
        // It reads from StreamingAssets folder also:
        byte[] file = FileManagement.ReadRawFile(name);
        // Calculate the partial file's start index:
        int begin = (part - 1) * maxChunkSize;
        // Set the buffer size acordingly:
        int size = maxChunkSize;    // Maximum size by default.
        if ((begin + maxChunkSize) > file.Length)
            size = file.Length - begin;     // When the partial file is less than chunk size.
        byte[] buffer = new byte[size];
        // Extract the partial file:
        System.Buffer.BlockCopy(file, begin, buffer, 0, buffer.Length);
        return buffer;
    }
    /// <summary>Send a partial file</summary>
    void SendPartialFile(string ip, string name, string part = "1")
    {
        // Verify if file exists (also StreamingAssets folder):
        if (!FileManagement.FileExists(name))
        {
            SendString(ip, "F2;" + GlobalDefinitions.thisComputerIPAddress + ";" + name + ";#");
            return;
        }
        // Configure the partial file message:
        int parts = GetFileParts(name);   // Total parts count of the requested file.
        string tempCmd = "F4;" + GlobalDefinitions.thisComputerIPAddress + ";" + name + ";" + part + "," + parts.ToString() + ";";
        byte[] cmd = new byte[tempCmd.Length];
        for (int c = 0; c < cmd.Length; c++)
            cmd[c] = (byte)tempCmd[c];
        // Reads the requested partial file:
        byte[] file = ReadPartialFile(name, int.Parse(part));
        // Adds the partial file to the message:
        byte[] data = new byte[cmd.Length + file.Length + 2];
        // Compose the message (header + file + endOfMessage):
        System.Buffer.BlockCopy(cmd, 0, data, 0, cmd.Length);
        System.Buffer.BlockCopy(file, 0, data, cmd.Length, file.Length);
        // Command ending:
        data[data.Length - 2] = (byte)';';
        //i++;
        data[data.Length - 1] = (byte)'#';
        // Send the partial file message:
        SendData(ip, data);
    }
    /// <summary>Sends the "file update" batch list</summary>
    void SendUpdateList(string ip, string file)
    {
        // The "file update" list file must be created manually (it can go into StreamingAssets):
        if (FileManagement.FileExists(file, true))
        {
            string listFile = FileManagement.ReadFile<string>(file);
            string[] list = listFile.Split(';');
            string cmd = "F7;" + GlobalDefinitions.thisComputerIPAddress + ";";
            for (int c = 0; c < list.Length; c++)
            {
                if (list[c] != "")
                {
                    cmd += list[c];
                    cmd += ";";
                }
            }
            cmd += "#";
            SendString(ip, cmd);
        }
        else
        {
            SendString(ip, "F5;" + GlobalDefinitions.thisComputerIPAddress + ";" + name + ";#");
        }
    }
    /// <summary>Calculates the partial files count</summary>
    int GetFileParts(string name)
    {
        int parts = Mathf.CeilToInt(FileManagement.ReadRawFile(name).Length / (float)maxChunkSize);
        return parts;
    }
    /// <summary>Sends a file to a known client (Server starts the transference request)</summary>
    public void SendFile(string ip, string name)
    {
        if (enableServer)
        {
            if (FileManagement.FileExists(name))
            {
                string cmd = "F8;" + GlobalDefinitions.thisComputerIPAddress + ";" + name + ";#";    // F8;ServerIP;name;#
                SendString(ip, cmd);
            }
            else
            {
                GlobalDefinitions.WriteToLogFile("Error - SendFile: File not found: " + name);
            }
        }
    }
    /// <summary>Sends the updatelist to a known client (Server starts the transference request)</summary>
    public void SendUpdate(string ip, string list)
    {
        if (enableServer)
        {
            if (FileManagement.FileExists(list))
            {
                string cmd = "F9;" + GlobalDefinitions.thisComputerIPAddress + ";" + list + ";#";    // F9;ServerIP;list;#
                SendString(ip, cmd);
            }
            else
            {
                //GlobalDefinitions.WriteToLogFile("[FileTransferServer.SendUpdate] List not found: " + list);
            }
        }
    }

    /**********************
     * Progress interfaces:
     **********************/
    /// <summary>Returns the name of the download file in process ("" if nothing)</summary>
    public string GetCurrentFile()
    {
        if (downloadList.Count > 0)
            return downloadList[0].file;
        else
            return "";
    }
    /// <summary>Returns the actual file part download in process "part/total" ("" if nothing)</summary>
    public string GetCurrentPartialStatus()
    {
        if (partialFileList.Length > 0)
        {
            int parts = 0;
            for (int c = 0; c < partialFileList.Length; c++)
            {
                if (partialFileList[c] != null && partialFileList[c] != "")
                    parts++;
            }
            return parts.ToString() + "/" + partialFileList.Length.ToString();
        }
        // If no files or all done:
        return "";
    }
    /// <summary>Returns the actual file download progress (0f to 1f)</summary>
    public float GetCurrentPartialProgress()
    {
        float val = 100f;   // If no file or file completed defaults 100%.
        if (partialFileList.Length > 0)
        {
            int parts = 0;
            for (int c = 0; c < partialFileList.Length; c++)
            {
                if (partialFileList[c] != null && partialFileList[c] != "")
                    parts++;
            }
            val = (float)parts / (float)partialFileList.Length;
        }
        // If no files or all done:
        return val;
    }

}
