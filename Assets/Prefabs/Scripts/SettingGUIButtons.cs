using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingGUIButtons : MonoBehaviour
{
    /// <summary>
    /// Called when the cancel button is selected
    /// </summary>
    public void cancelSelected()
    {
        // Get rid of the gui
        GlobalDefinitions.removeGUI(transform.parent.gameObject);

        // Bring back any gui's that were active before this was called
        foreach (GameObject gui in GlobalDefinitions.guiList)
            gui.SetActive(true);

        // Turn the button back on
        GameObject.Find("SettingsButton").GetComponent<Button>().interactable = true;
    }

    /// <summary>
    /// Called when the ok button is selected
    /// </summary>
    public void okSelected()
    {
        GlobalDefinitions.aggressiveSetting = (int)GameObject.Find("AgressivenessSlider").GetComponent<Slider>().value;
        GlobalDefinitions.difficultySetting = (int)GameObject.Find("DifficultySlider").GetComponent<Slider>().value;
        // Write out the values of the sliders to the settings file
        GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().writeSettingsFile(GlobalDefinitions.difficultySetting, GlobalDefinitions.aggressiveSetting);
        CombatResolutionRoutines.adjustAggressiveness();

        cancelSelected();
    }
}
