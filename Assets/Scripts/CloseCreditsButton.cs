using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseCreditsButton : MonoBehaviour
{
    public void CloseCredits() {
        GameLoader.Instance.LoadMenuScene();
    }
}
