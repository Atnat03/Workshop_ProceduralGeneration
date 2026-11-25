using MyPrint;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class TreeAnimation : MonoBehaviour
{
    public float durationAppear = 0.15f;
    public float durationBurn = 1f;

    private double startTimeAppear;
    private double startTimeBurning;
    
    private bool isAnimating;
    private bool isBurning;
    
    private ParticleSystem _burnVFX;

    void OnEnable()
    {
        _burnVFX = Resources.Load<GameObject>("BurnVFX").GetComponent<ParticleSystem>();
        
        if(_burnVFX == null)
            ABPrint.Print("No BurnVFX found", ABColor.Orange);
    }

    #region Appear
    
    public void PlayAppear()
    {
        transform.localScale = Vector3.zero;

        startTimeAppear = EditorApplication.timeSinceStartup;
        isAnimating = true;

        EditorApplication.update += UpdateAnimation;
    }

    private void UpdateAnimation()
    {
        if (isAnimating)
        {
            AppearAnimation();
        }

        if (isBurning)
        {
            UpdateBurn();
        }
    }

    private void AppearAnimation()
    {
        if (this == null)
        {
            StopAnimation();
            return;
        }
        
        float time = (float)(EditorApplication.timeSinceStartup - startTimeAppear);

        float t = Mathf.Clamp01(time / durationAppear);

        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

        if (t >= 1f)
        {
            transform.localScale = Vector3.one;
            StopAnimation();
        }

        SceneView.RepaintAll();
    }
    
    #endregion

    #region Burn
    
    public void StartDisappear()
    {
        isBurning = true;
        startTimeBurning =  EditorApplication.timeSinceStartup;

        if (_burnVFX)
        {
            Instantiate(_burnVFX, transform).Play();
        }
        
        EditorApplication.update += UpdateAnimation;
    }
    
    private void UpdateBurn()
    {
        if (this == null)
        {
            StopAnimation();
            return;
        }
        
        float time = (float)(EditorApplication.timeSinceStartup - startTimeBurning);

        float t = Mathf.Clamp01(time / durationBurn);

        transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);

        if (t >= 1f)
        {
            transform.localScale = Vector3.zero;
         
            DestroyImmediate(gameObject);
            
            StopAnimation();
        }
        
        SceneView.RepaintAll();
    }
    
    #endregion
    
    private void StopAnimation()
    {
        isAnimating = false;
        isBurning = false;
        EditorApplication.update -= UpdateAnimation;
    }
    
    private void OnDisable()
    {
        StopAnimation();
    }
}
