using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//操作键管理（单例模式）
public class InputManager
{
    private static InputManager instance;

    private InputManager()
    {
        upKey = KeyCode.UpArrow;
        downKey = KeyCode.DownArrow;
        leftKey = KeyCode.LeftArrow;
        rightKey = KeyCode.RightArrow;
        jumpKey = KeyCode.Z;
        attackKey = KeyCode.X;
        sprintKey = KeyCode.C;
        superSprintKey = KeyCode.Space;
        menuKey = KeyCode.Escape;
    }

    public static InputManager Instance
    {
        get
        {
            if (instance == null)
                instance = new InputManager();
            return instance;
        }
    }

    #region 按键

    /// <summary>
    /// 上方向键
    /// </summary>
    public KeyCode upKey;

    /// <summary>
    /// 下方向键
    /// </summary>
    public KeyCode downKey;

    /// <summary>
    /// 左方向键
    /// </summary>
    public KeyCode leftKey;

    /// <summary>
    /// 右方向键
    /// </summary>
    public KeyCode rightKey;

    /// <summary>
    /// 跳跃键
    /// </summary>
    public KeyCode jumpKey;

    /// <summary>
    /// 攻击键
    /// </summary>
    public KeyCode attackKey;

    /// <summary>
    /// 冲刺键
    /// </summary>
    public KeyCode sprintKey;

    /// <summary>
    /// 超级冲刺键
    /// </summary>
    public KeyCode superSprintKey;

    /// <summary>
    /// 菜单键
    /// </summary>
    public KeyCode menuKey;

    #endregion


}
