using System.Collections;
using UnityEngine;

public class ControlUIScript : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(DisableControlUI());
    }

    private IEnumerator DisableControlUI()
    {
        yield return new WaitForSeconds(15f);
        this.gameObject.SetActive(false);
    }
}
