using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionDetect : MonoBehaviour
{
    [SerializeField] GameObject thePlayer;
    [SerializeField] GameObject playerAnim;
    [SerializeField] AudioSource collisionFX;
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject fadeOut;

    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(CollisionEnd());
    }

    IEnumerator CollisionEnd()
    {
        if (collisionFX != null)
        {
            collisionFX.Play();
        }
        
        if (thePlayer != null)
        {
            thePlayer.GetComponent<PlayerMovement>().enabled = false;
        }
        
        if (playerAnim != null)
        {
            playerAnim.GetComponent<Animator>().Play("Stumble Backwards");
        }
        
        if (mainCam != null)
        {
            mainCam.GetComponent<Animator>().Play("CollisionCam");
        }
        
        yield return new WaitForSeconds(3);
        
        if (fadeOut != null)
        {
            fadeOut.SetActive(true);
        }
        
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(0);
    }
}