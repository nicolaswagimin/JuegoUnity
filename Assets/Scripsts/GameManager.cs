using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject fadeOut;
    private bool gameWon = false;
    
    // Secuencia de escenas: Selva -> Desierto -> Nieve -> MainMenu
    private string[] sceneSequence = { "Selva", "Desierto", "Nieve", "MainMenu" };
    
    // Variable estática para rastrear el progreso de las escenas
    public static int sceneProgress = 0;
    
    void Start()
    {
        // Verificar que no estamos en el menú principal
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("[GameManager] Start - Escena actual: " + currentScene);
        
        if (currentScene == "MainMenu")
        {
            // No hacer nada si estamos en el menú principal
            return;
        }
        
        // Sincronizar sceneProgress con la escena actual
        for (int i = 0; i < sceneSequence.Length; i++)
        {
            if (sceneSequence[i] == currentScene)
            {
                sceneProgress = i;
                Debug.Log("[GameManager] Sincronizado sceneProgress a: " + sceneProgress + " para escena: " + currentScene);
                break;
            }
        }
        
        // Resetear el flag cuando se carga una nueva escena
        gameWon = false;
        // Resetear el contador de monedas al iniciar una nueva escena
        MasterInfo.coinCount = 0;
        
        Debug.Log("[GameManager] Escena: " + currentScene + " - Progreso: " + sceneProgress + " - Monedas reseteadas a 0");
    }
    
    void Update()
    {
        // Verificar que no estamos en el menú principal
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu")
        {
            return;
        }
        
        // Verificar si se alcanzaron 15 monedas
        if (!gameWon && MasterInfo.coinCount >= 15)
        {
            Debug.Log("[GameManager] ¡15 monedas alcanzadas! Monedas actuales: " + MasterInfo.coinCount);
            gameWon = true;
            StartCoroutine(WinGame());
        }
    }
    
    IEnumerator WinGame()
    {
        Debug.Log("[GameManager] WinGame iniciado - Monedas: " + MasterInfo.coinCount);
        
        // Activar fadeOut y reproducir la animación si existe
        if (fadeOut != null)
        {
            // PRIMERO activar el GameObject para que Unity inicialice todos los componentes
            fadeOut.SetActive(true);
            Debug.Log("[GameManager] GameObject fadeOut activado");
            
            // Esperar un frame para que Unity inicialice completamente el GameObject y sus componentes
            yield return null;
            
            // AHORA obtener el componente Animator después de que el GameObject esté activo
            Animator fadeOutAnimator = fadeOut.GetComponent<Animator>();
            
            if (fadeOutAnimator != null)
            {
                Debug.Log("[GameManager] Animator encontrado después de activar GameObject");
                
                // Asegurarse de que el Animator esté habilitado
                if (!fadeOutAnimator.enabled)
                {
                    fadeOutAnimator.enabled = true;
                    Debug.Log("[GameManager] Animator habilitado");
                }
                
                // Verificar que el controlador esté asignado
                if (fadeOutAnimator.runtimeAnimatorController != null)
                {
                    Debug.Log("[GameManager] Controlador encontrado: " + fadeOutAnimator.runtimeAnimatorController.name);
                    
                    // Esperar otro frame para que el Animator se inicialice completamente
                    yield return null;
                    
                    // Forzar la inicialización del Animator
                    fadeOutAnimator.Update(0f);
                    
                    // Intentar reproducir la animación - usar PlayInFixedTime que es más confiable
                    int stateHash = Animator.StringToHash("FadeOut");
                    fadeOutAnimator.PlayInFixedTime("FadeOut", 0, 0f);
                    
                    Debug.Log("[GameManager] Animación FadeOut iniciada con PlayInFixedTime. Hash: " + stateHash);
                    
                    // Esperar un frame más para que la animación comience
                    yield return null;
                    
                    // Verificar que la animación se está reproduciendo
                    AnimatorStateInfo stateInfo = fadeOutAnimator.GetCurrentAnimatorStateInfo(0);
                    Debug.Log("[GameManager] Estado actual - Nombre: " + stateInfo.fullPathHash + ", NormalizedTime: " + stateInfo.normalizedTime + ", Length: " + stateInfo.length);
                    
                    // Verificar si está en el estado correcto o intentar de nuevo
                    if (stateInfo.length > 0 && stateInfo.normalizedTime >= 0)
                    {
                        Debug.Log("[GameManager] Animación confirmada reproduciéndose correctamente");
                    }
                    else
                    {
                        Debug.LogWarning("[GameManager] La animación no parece estar reproduciéndose. Intentando método alternativo...");
                        
                        // Método alternativo: usar Play directamente
                        fadeOutAnimator.Rebind();
                        yield return null;
                        fadeOutAnimator.Play("FadeOut", 0, 0f);
                        Debug.Log("[GameManager] Intentado con Play() después de Rebind");
                    }
                }
                else
                {
                    Debug.LogError("[GameManager] El Animator no tiene un controlador asignado!");
                }
            }
            else
            {
                Debug.LogError("[GameManager] No se encontró componente Animator en fadeOut después de activarlo");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] FadeOut no está asignado, continuando sin fade");
        }
        
        // Esperar a que la animación termine (duración: 3 segundos)
        yield return new WaitForSeconds(3f);
        
        // Cargar la siguiente escena en la secuencia
        LoadNextScene();
    }
    
    void LoadNextScene()
    {
        // Avanzar al siguiente índice en la secuencia
        sceneProgress++;
        Debug.Log("[GameManager] LoadNextScene - sceneProgress incrementado a: " + sceneProgress);
        
        if (sceneProgress < sceneSequence.Length)
        {
            string sceneName = sceneSequence[sceneProgress];
            Debug.Log("[GameManager] Cambiando a escena: " + sceneName + " (Índice: " + sceneProgress + ")");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            // Si ya pasamos por todas las escenas, volver al menú y resetear todo
            Debug.Log("[GameManager] ¡Juego completado! Volviendo al menú principal.");
            sceneProgress = 0;
            MasterInfo.coinCount = 0;
            SceneManager.LoadScene("MainMenu");
        }
    }
}