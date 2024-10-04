using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialMediaLinks : MonoBehaviour
{
    private string _telegramURL = "https://t.me/I_nYn_I";

    public void OpenTelegram()
    {
        Application.OpenURL(_telegramURL);
    }
}
