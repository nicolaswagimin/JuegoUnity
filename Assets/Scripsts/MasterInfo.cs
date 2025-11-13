using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterInfo : MonoBehaviour
{
    public static int coinCount = 0;
    [SerializeField] GameObject coinDisplay;
    [SerializeField] GameObject fadeOut;
    
    private bool gameWon = false;
    public static int sceneProgress = 0;
    private string[] sceneSequence = { "Selva", "Desierto", "Nieve", "MainMenu" };
    
    // Método auxiliar para buscar recursivamente en la jerarquía
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            Transform result = FindChildRecursive(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    void Start()
    {
        // Verificar que no estamos en el menú principal
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu")
        {
            return;
        }
        
        // Sincronizar sceneProgress con la escena actual
        for (int i = 0; i < sceneSequence.Length; i++)
        {
            if (sceneSequence[i] == currentScene)
            {
                sceneProgress = i;
                break;
            }
        }
        
        // Resetear el flag y contador al iniciar una nueva escena
        gameWon = false;
        coinCount = 0;
        
        // Buscar el fadeOut automáticamente si no está asignado
        if (fadeOut == null)
        {
            Debug.LogWarning("[MasterInfo] fadeOut no está asignado. Buscando automáticamente...");
            
            // Buscar en todos los GameObjects de la escena, incluso los desactivados
            // Primero intentar con Find (solo encuentra activos)
            GameObject foundFadeOut = GameObject.Find("FadeOut");
            
            // Si no se encuentra, buscar en todos los objetos de la escena
            if (foundFadeOut == null)
            {
                // Buscar recursivamente en todos los objetos de la escena
                GameObject[] allObjects = FindObjectsOfType<GameObject>(true); // true = incluir inactivos
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "FadeOut")
                    {
                        foundFadeOut = obj;
                        Debug.Log("[MasterInfo] fadeOut encontrado en objetos inactivos: " + obj.name);
                        break;
                    }
                }
            }
            
            // Si aún no se encuentra, buscar en el Canvas
            if (foundFadeOut == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>(true); // true = incluir inactivos
                if (canvas != null)
                {
                    // Buscar recursivamente en los hijos del Canvas
                    Transform fadeOutTransform = FindChildRecursive(canvas.transform, "FadeOut");
                    if (fadeOutTransform != null)
                    {
                        foundFadeOut = fadeOutTransform.gameObject;
                        Debug.Log("[MasterInfo] fadeOut encontrado en Canvas: " + foundFadeOut.name);
                    }
                }
            }
            
            if (foundFadeOut != null)
            {
                fadeOut = foundFadeOut;
                Debug.Log("[MasterInfo] fadeOut encontrado y asignado: " + fadeOut.name + " (Activo: " + fadeOut.activeSelf + ")");
            }
            else
            {
                Debug.LogError("[MasterInfo] No se pudo encontrar el GameObject fadeOut en la escena " + currentScene);
            }
        }
        else
        {
            Debug.Log("[MasterInfo] fadeOut está asignado correctamente: " + fadeOut.name + " (Activo: " + fadeOut.activeSelf + ")");
        }
        
        Debug.Log("[MasterInfo] Escena: " + currentScene + " - Progreso: " + sceneProgress + " - Monedas reseteadas");
    }

    void Update()
    {
        // Actualizar el display de monedas
        if (coinDisplay != null)
        {
            coinDisplay.GetComponent<TMPro.TMP_Text>().text = "COINS: " + coinCount;
        }
        
        // Verificar que no estamos en el menú principal
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu")
        {
            return;
        }
        
        // Verificar si se alcanzaron 15 monedas
        if (!gameWon && coinCount >= 15)
        {
            Debug.Log("[MasterInfo] ¡15 monedas alcanzadas! Cambiando de escena...");
            gameWon = true;
            StartCoroutine(ChangeScene());
        }
    }
    
    IEnumerator ChangeScene()
    {
        Debug.Log("[MasterInfo] Iniciando cambio de escena - Monedas: " + coinCount);
        
        // Activar fadeOut y reproducir la animación si existe
        if (fadeOut != null)
        {
            // PRIMERO activar el GameObject para que Unity inicialice todos los componentes
            fadeOut.SetActive(true);
            Debug.Log("[MasterInfo] GameObject fadeOut activado");
            
            // Esperar un frame para que Unity inicialice completamente el GameObject y sus componentes
            yield return null;
            
            // AHORA obtener el componente Animator después de que el GameObject esté activo
            Animator fadeOutAnimator = fadeOut.GetComponent<Animator>();
            
            if (fadeOutAnimator != null)
            {
                Debug.Log("[MasterInfo] Animator encontrado después de activar GameObject");
                
                // Asegurarse de que el Animator esté habilitado
                if (!fadeOutAnimator.enabled)
                {
                    fadeOutAnimator.enabled = true;
                    Debug.Log("[MasterInfo] Animator habilitado");
                }
                
                // Verificar que el controlador esté asignado
                if (fadeOutAnimator.runtimeAnimatorController != null)
                {
                    Debug.Log("[MasterInfo] Controlador encontrado: " + fadeOutAnimator.runtimeAnimatorController.name);
                    
                    // Esperar otro frame para que el Animator se inicialice completamente
                    yield return null;
                    
                    // Forzar la inicialización del Animator
                    fadeOutAnimator.Update(0f);
                    
                    // Intentar reproducir la animación - usar PlayInFixedTime que es más confiable
                    int stateHash = Animator.StringToHash("FadeOut");
                    fadeOutAnimator.PlayInFixedTime("FadeOut", 0, 0f);
                    
                    Debug.Log("[MasterInfo] Animación FadeOut iniciada con PlayInFixedTime. Hash: " + stateHash);
                    
                    // Esperar un frame más para que la animación comience
                    yield return null;
                    
                    // Verificar que la animación se está reproduciendo
                    AnimatorStateInfo stateInfo = fadeOutAnimator.GetCurrentAnimatorStateInfo(0);
                    Debug.Log("[MasterInfo] Estado actual - Nombre: " + stateInfo.fullPathHash + ", NormalizedTime: " + stateInfo.normalizedTime + ", Length: " + stateInfo.length);
                    
                    // Verificar si está en el estado correcto o intentar de nuevo
                    if (stateInfo.length > 0 && stateInfo.normalizedTime >= 0)
                    {
                        Debug.Log("[MasterInfo] Animación confirmada reproduciéndose correctamente");
                    }
                    else
                    {
                        Debug.LogWarning("[MasterInfo] La animación no parece estar reproduciéndose. Intentando método alternativo...");
                        
                        // Método alternativo: usar Play directamente
                        fadeOutAnimator.Rebind();
                        yield return null;
                        fadeOutAnimator.Play("FadeOut", 0, 0f);
                        Debug.Log("[MasterInfo] Intentado con Play() después de Rebind");
                    }
                }
                else
                {
                    Debug.LogError("[MasterInfo] El Animator no tiene un controlador asignado!");
                }
            }
            else
            {
                Debug.LogError("[MasterInfo] No se encontró componente Animator en fadeOut después de activarlo");
            }
        }
        else
        {
            Debug.LogError("[MasterInfo] fadeOut GameObject es null!");
        }
        
        // Esperar a que la animación termine (duración: 3 segundos)
        yield return new WaitForSeconds(3f);
        
        // Avanzar al siguiente índice en la secuencia
        sceneProgress++;
        
        if (sceneProgress < sceneSequence.Length)
        {
            string sceneName = sceneSequence[sceneProgress];
            Debug.Log("[MasterInfo] Cambiando a escena: " + sceneName + " (Índice: " + sceneProgress + ")");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            // Si ya pasamos por todas las escenas, volver al menú y resetear todo
            Debug.Log("[MasterInfo] ¡Juego completado! Volviendo al menú principal.");
            sceneProgress = 0;
            coinCount = 0;
            SceneManager.LoadScene("MainMenu");
        }
    }
}