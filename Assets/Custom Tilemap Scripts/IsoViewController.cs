/*  IsoViewController.cs
 *  把它挂到任意 GameObject 上即可。
 *  提供：
 *    – SwitchCW() / SwitchCCW()  让 UI 按钮或键盘调用
 *    – CurrentViewIndex  随时读当前视图索引 (0-3)
 *    - OnRotateProgress 事件，提供旋转进度和方向
 *  
 *  控制四向视图平滑切换：NE、NW、SW、SE
 */

using UnityEngine;
using System.Collections;

public class IsoViewController : MonoBehaviour
{
    [Header("控制器引用")]
    [SerializeField] private TileMapController map; // 拖入TileMapController引用
    
    [Header("键盘控制")]
    [SerializeField] private KeyCode viewClockwiseKey = KeyCode.E;      // 顺时针切换
    [SerializeField] private KeyCode viewCounterClockwiseKey = KeyCode.Q;  // 逆时针切换

    [Header("旋转动画")]
    [SerializeField] private float rotateDuration = 0.25f;  // 旋转持续时间
    [SerializeField] private AnimationCurve rotateCurve = 
        AnimationCurve.EaseInOut(0, 0, 1, 1);  // 旋转动画曲线

    // 供外部读取
    public int CurrentViewIndex { get; private set; } = 0;  // 0-NE, 1-NW, 2-SW, 3-SE

    /* 事件：参数 (t, dir)
       t ∈ [0,1] 插值因子；dir = +1(CCW) / -1(CW) */
    public event System.Action<float, int> OnRotateProgress;

    private void Update()
    {
        // 检测键盘输入控制视图切换
        if (Input.GetKeyDown(viewClockwiseKey))
        {
            SwitchCW();
        }
        else if (Input.GetKeyDown(viewCounterClockwiseKey))
        {
            SwitchCCW();
        }
    }

    /* UI 或键盘调用 */
    public void SwitchCW()
    {
        if(!_busy)
        {
            if (map != null)
                map.SetPivotByScreen(new Vector2(Screen.width/2, Screen.height/2));  // 把屏幕中心设为 pivot
            StartCoroutine(RotateStep(-1));
        }
    }
    
    public void SwitchCCW()
    {
        if(!_busy)
        {
            if (map != null)
                map.SetPivotByScreen(new Vector2(Screen.width/2, Screen.height/2));  // 把屏幕中心设为 pivot
            StartCoroutine(RotateStep(+1));
        }
    }

    /* --------------------- 私有 --------------------- */
    private bool _busy;

    private IEnumerator RotateStep(int delta)                       // delta = ±1
    {
        _busy = true;

        int from = CurrentViewIndex;
        int to   = (CurrentViewIndex + delta + 4) % 4;

        /* ★ 先通知一次 t=0 ★ */
        OnRotateProgress?.Invoke(0f, delta);

        float t = 0f;
        while (t < rotateDuration)
        {
            t += Time.deltaTime;
            float k = rotateCurve.Evaluate(t / rotateDuration);
            OnRotateProgress?.Invoke(k, delta);
            yield return null;
        }

        CurrentViewIndex = to;
        OnRotateProgress?.Invoke(1f, delta);                // 确保收尾
        _busy = false;
    }
}
