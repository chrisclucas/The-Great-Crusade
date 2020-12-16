using UnityEngine;
using UnityEngine.UI;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class SettingGUIButtons : MonoBehaviour
    {
        /// <summary>
        /// Called when the cancel button is selected
        /// </summary>
        public void CancelSelected()
        {
            // Get rid of the gui
            GUIRoutines.RemoveGUI(transform.parent.gameObject);

            // Bring back any gui's that were active before this was called
            foreach (GameObject gui in GUIRoutines.guiList)
                gui.SetActive(true);

            // Turn the button back on
            GameObject.Find("SettingsButton").GetComponent<Button>().interactable = true;
        }

        /// <summary>
        /// Called when the ok button is selected
        /// </summary>
        public void OkSelected()
        {
            GlobalDefinitions.aggressiveSetting = (int)GameObject.Find("AgressivenessSlider").GetComponent<Slider>().value;
            GlobalDefinitions.difficultySetting = (int)GameObject.Find("DifficultySlider").GetComponent<Slider>().value;
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.AGGRESSIVESETTINGKEYWORD + " " + (int)GameObject.Find("AgressivenessSlider").GetComponent<Slider>().value);
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.DIFFICULTYSETTINGKEYWORD + " " + (int)GameObject.Find("DifficultySlider").GetComponent<Slider>().value);
            // Write out the values of the sliders to the settings file
            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().WriteSettingsFile(GlobalDefinitions.difficultySetting, GlobalDefinitions.aggressiveSetting);
            CombatResolutionRoutines.AdjustAggressiveness();

            CancelSelected();
        }
    }
}