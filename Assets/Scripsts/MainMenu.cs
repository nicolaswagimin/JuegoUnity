using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuControl : MonoBehaviour
{
    [SerializeField] GameObject fadeOut;

    void Start()
    {
        Debug.Log("[MainMenu] MainMenuControl iniciado. FadeOut asignado: " + (fadeOut != null));
    }

    void Update()
    {
    }

    public void StartGame()
    {
        Debug.Log("[MainMenu] StartGame() llamado desde el botón!");
        StartCoroutine(StartButton());
    }

    IEnumerator StartButton()
    {
        Debug.Log("[MainMenu] Iniciando juego...");
        
        // Activar fadeOut y reproducir la animación si existe
        if (fadeOut != null)
        {
            // PRIMERO activar el GameObject para que Unity inicialice todos los componentes
            fadeOut.SetActive(true);
            Debug.Log("[MainMenu] GameObject fadeOut activado");
            
            // Esperar un frame para que Unity inicialice completamente el GameObject y sus componentes
            yield return null;
            
            // AHORA obtener el componente Animator después de que el GameObject esté activo
            Animator fadeOutAnimator = fadeOut.GetComponent<Animator>();
            
            if (fadeOutAnimator != null)
            {
                Debug.Log("[MainMenu] Animator encontrado después de activar GameObject");
                
                // Asegurarse de que el Animator esté habilitado
                if (!fadeOutAnimator.enabled)
                {
                    fadeOutAnimator.enabled = true;
                    Debug.Log("[MainMenu] Animator habilitado");
                }
                
                // Verificar que el controlador esté asignado
                if (fadeOutAnimator.runtimeAnimatorController != null)
                {
                    string controllerName = fadeOutAnimator.runtimeAnimatorController.name;
                    Debug.Log("[MainMenu] Controlador encontrado: " + controllerName);
                    
                    // Determinar qué animación usar según el controlador
                    string animationName = "";
                    if (controllerName.Contains("FadeOut 1") || controllerName.Contains("MMFadeOut"))
                    {
                        animationName = "MMFadeOut";
                    }
                    else
                    {
                        animationName = "FadeOut";
                    }
                    
                    // Esperar otro frame para que el Animator se inicialice completamente
                    yield return null;
                    
                    // Forzar la inicialización del Animator
                    fadeOutAnimator.Update(0f);
                    
                    // Intentar reproducir la animación - usar PlayInFixedTime que es más confiable
                    int stateHash = Animator.StringToHash(animationName);
                    fadeOutAnimator.PlayInFixedTime(animationName, 0, 0f);
                    
                    Debug.Log("[MainMenu] Animación " + animationName + " iniciada con PlayInFixedTime. Hash: " + stateHash);
                    
                    // Esperar un frame más para que la animación comience
                    yield return null;
                    
                    // Verificar que la animación se está reproduciendo
                    AnimatorStateInfo stateInfo = fadeOutAnimator.GetCurrentAnimatorStateInfo(0);
                    Debug.Log("[MainMenu] Estado actual - Nombre: " + stateInfo.fullPathHash + ", NormalizedTime: " + stateInfo.normalizedTime + ", Length: " + stateInfo.length);
                    
                    // Verificar si está en el estado correcto o intentar de nuevo
                    if (stateInfo.length > 0 && stateInfo.normalizedTime >= 0)
                    {
                        Debug.Log("[MainMenu] Animación " + animationName + " confirmada reproduciéndose correctamente");
                    }
                    else
                    {
                        Debug.LogWarning("[MainMenu] La animación no parece estar reproduciéndose. Intentando método alternativo...");
                        
                        // Método alternativo: usar Play directamente
                        fadeOutAnimator.Rebind();
                        yield return null;
                        fadeOutAnimator.Play(animationName, 0, 0f);
                        Debug.Log("[MainMenu] Intentado con Play() después de Rebind");
                    }
                }
                else
                {
                    Debug.LogError("[MainMenu] El Animator no tiene un controlador asignado!");
                }
            }
            else
            {
                Debug.LogError("[MainMenu] No se encontró componente Animator en fadeOut después de activarlo");
            }
        }
        else
        {
            Debug.LogWarning("[MainMenu] FadeOut no está asignado!");
        }
        
        // Esperar a que la animación termine (duración: 3 segundos)
        yield return new WaitForSeconds(3f);
        
        // Resetear el progreso de escenas al iniciar un nuevo juego
        // La secuencia es: Selva -> Desierto -> Nieve -> MainMenu
        MasterInfo.sceneProgress = 0;
        MasterInfo.coinCount = 0;
        
        Debug.Log("[MainMenu] Cargando escena Selva...");
        // Iniciar en la primera escena de la secuencia: Selva
        SceneManager.LoadScene("Selva");
    }
}