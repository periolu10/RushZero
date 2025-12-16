using UnityEngine;

public class Scenes : MonoBehaviour
{
    // The sages don't have to be in order, but PLEASE make sure to assign an INT next to-
    // the name so we know what the flip stage we're in

    public enum Scene
    {
        // UI and MENUS
        ui_TitleScreen = 0,
        ui_MainMenu = 1,

        // HUBS
        hub_Village = 4,

        // STAGES
        stage_Tutorial = 6,
        stage_WildvinePath = 7,
        stage_ForestPath = 9,

        // BOSS
        boss_Forest = 10,

        // CUTSCENES
        ___ = 201,
        CUTSCENES = 202,
        ____ = 203,
        ctsn_Intro = 8,

        // TEST SCENES
        _________________ = 999,
        TEST_SCENES = 1000,
        ________________ = 1001,
        stage_TestAction = 2,
        stage_TestRunner = 3,
        room_hub_Test1 = 5,
    }
}
