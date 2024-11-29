using System.Collections;
using System.Collections.Generic;
using Ubiq.Avatars.Rocketbox;
using UnityEngine;
using UnityEngine.UI;

public class AvatarListHelper : MonoBehaviour
{
    public RocketboxManager rocketboxManager;
    public RocketboxAvatar avatar;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in GetComponentsInChildren<Button>())
        {
            item.onClick.AddListener(() =>
            {
                StartCoroutine(rocketboxManager.LoadAvatarAsync(item.GetComponentInChildren<TMPro.TextMeshProUGUI>().text, avatar));
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
