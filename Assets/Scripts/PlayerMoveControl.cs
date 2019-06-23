using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerMoveControl : MonoBehaviour
{
    #region PRIVATE PARA

    bool isAlive;//是否存活
    bool isGround;//是否在地面
    bool isClimb;   //是否在爬墙
    bool isJump;   //是否在跳跃
    Vector2 moveSpeed;  //每一帧的移动速度
    Vector2 boxSize;    //盒子射线的大小
    Animator animator;//动画控制器
    int jumpCount;  //跳跃次数
    float jumpTime; //跳跃时长
    int playerLayerMask;    //非玩家层级，用于碰撞检测忽略玩家本身

    #endregion

    #region PUBLIC PARA

    public bool inputEnable = true;   //接受输入的开关：true接受用户输入操作角色；false不接受输入
    public bool gravityEnable = true;
    public bool isCanSprint = true;   //是否能冲刺
    public bool isShadowSprint = true;
    public float speed = 6.5f;  //移动速度
    public float gravity = 28f;  //收到的重力
    public float jumpSpeed = 2f; //跳跃速度
    public float jumpMaxTime = 0.11f;   //跳跃最大时长
    public float sprintTime = 0.1f; //冲刺持续时间
    public float minDistance = 0.15f;  //碰撞移动的最小距离
    public GameObject jumpTwiceEffect;

    #endregion

    // 初始化参数
    void Start()
    {
        isAlive = true;
        isGround = true;
        isClimb = false;
        isJump = false;

        jumpCount = 0;
        jumpTime = 0;

        moveSpeed = new Vector2(0, 0);
        boxSize = new Vector2(0.66f, 1.32f);

        playerLayerMask = LayerMask.GetMask("Player");
        playerLayerMask = ~playerLayerMask;             //获得当前玩家层级的mask值，并使用~运算，让射线忽略玩家层检测

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isAlive)
        {
            return; //若已死亡不做任何处理
        }

        LRMove();   //左右移动
        UpdatePlayerAnimatorState();
        UpdateGravity();
        Jump();
		Attack();
        Sprint();

        animator.SetBool("isGround", isGround);
        animator.ResetTrigger("setClimb");
        NextFrameMove();
    }

    #region LRMove

    /// <summary>
    /// 左右移动
    /// </summary>
    void LRMove()
    {
        if(!inputEnable)
        {
            return;//不接受输入则返回
        }

        //根据输入设置移动速度
        if(Input.GetKey(InputManager.Instance.leftKey))
        {
            moveSpeed.x = -1 * speed;
        }
        else if(Input.GetKey(InputManager.Instance.rightKey))
        {
            moveSpeed.x = 1 * speed;
        }
        else
        {
            moveSpeed.x = 0;
        }

        if(moveSpeed.x == 0) //停止移动
        {
            animator.SetTrigger("setStop");
            animator.ResetTrigger("setRotate");
            animator.SetBool("isMove", false);
        }
        else
        {
            animator.ResetTrigger("setStop");
            if (!isClimb) //如果没有在爬墙
            {
                if (transform.localScale.x * moveSpeed.x > 0)//转向
                {
                    if (isGround)
                    {
                        animator.SetTrigger("setRotate");
                    }

                    transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
                }
                else
                {
                    animator.SetBool("isMove", true);
                }
            }
        }

        PlayRunAudio(moveSpeed.x);
    }

    /// <summary>
    /// 播放行走音效
    /// </summary>
    /// <param name="h"></param>
    void PlayRunAudio(float h)
    {

    }

    #endregion

    #region UpdatePlayerAnimatorState

    /// <summary>
    /// 更新玩家的状态信息
    /// </summary>
    void UpdatePlayerAnimatorState()
    {
        if(isGround) //如果在地面上
        {
            animator.SetBool("isJumpUp", false);
            animator.ResetTrigger("setJumpTwice");//重置2段跳触发器
            jumpCount = 0;
            animator.SetBool("isDown", false);
            isJump = false;
            isClimb = false;
            isCanSprint = true;
        }
        else
        {
            animator.SetBool("isDown", !isJump);
            if(isClimb)
            {
                jumpCount = 0;
            }
        }
    }

    #endregion

    #region UpdateGravity

    /// <summary>
    /// 更新重力
    /// </summary>
    void UpdateGravity()
    {
        if(!gravityEnable)
        {
            return;
        }

        if(isGround)
        {
            moveSpeed.y = 0;
        }
        else
        {
            if(isClimb)
            {
                moveSpeed.y = -1.0f;
            }
            else
            {
                moveSpeed.y += -1 * gravity * Time.deltaTime;
            }
        }
    }

    #endregion

    #region Jump

    void Jump()
    {
        if(!inputEnable) //不接受输入
        {
            return;
        }
        
        if(isClimb && Input.GetKeyDown(InputManager.Instance.jumpKey)) //爬墙跳跃
        {
            StartCoroutine(ClimbToJump());
            return;
        }

        if(Input.GetKeyDown(InputManager.Instance.jumpKey)) //正常跳跃
        {
            isJump = true;

            if(!isGround && jumpCount < 2)
            {
                jumpCount = 2;
            }
            else
            {
                jumpCount++;
            }

            if(jumpCount == 1)
            {
                moveSpeed.y += jumpSpeed;
                animator.SetBool("isJumpUp", true);
            }
            else if(jumpCount == 2)
            {
                moveSpeed.y = jumpSpeed;
                animator.SetTrigger("setJumpTwice");
                StartCoroutine(JumpTwiceEffect());
            }

            jumpTime = 0;
        }
        else if(Input.GetKey(InputManager.Instance.jumpKey) && isJump && jumpCount <= 2)
        {
            jumpTime += 0.02f;
            if (jumpTime < jumpMaxTime)
            {
                moveSpeed.y += jumpSpeed;
            }
        }
        else if(Input.GetKeyUp(InputManager.Instance.jumpKey))
        {
            isJump = false;
            jumpTime = 0;
        }

        //进入上跳减速状态，但还在上升
        if (moveSpeed.y > 0 && moveSpeed.y < 1.5f)
        {
            animator.SetBool("isSlowUp", true);
        }
        else
        {
            animator.SetBool("isSlowUp", false);
        }

        //进入下落状态
        if (moveSpeed.y < 0)
        {

            animator.SetBool("isJumpDown", true);
        }
        else
        {
            animator.SetBool("isJumpDown", false);

        }
    }

    IEnumerator JumpTwiceEffect()
    {
        jumpTwiceEffect.SetActive(true);
        yield return new WaitForSeconds(1f);
        jumpTwiceEffect.SetActive(false);

    }

    /// <summary>
    /// 墙上跳跃的移动
    /// </summary>
    /// <returns></returns>
    IEnumerator ClimbToJump()
    {
        inputEnable = false;    //此时不接受其余输入
        gravityEnable = false;
        isClimb = false;
        animator.SetTrigger("setClimbToJump");

        animator.ResetTrigger("setClimb");//重重爬墙触发器
        if(transform.localScale.x > 0 )
        {
            moveSpeed.x = 8f;
        }
        else
        {
            moveSpeed.x = -8f;
        }

        moveSpeed.y = 6;
        yield return new WaitForSeconds(0.15f);
        inputEnable = true;
        gravityEnable = true;

    }

    #endregion

    #region Attack

    void Attack()
    {
        if(!inputEnable || isClimb)
        {
            return;
        }
		
		if(Input.GetKeyDown(InputManager.Instance.attackKey))
		{
			if(Input.GetKey(InputManager.Instance.upKey))
            {
				animator.SetTrigger("setAttackUp");
            }
            else if (Input.GetKey(InputManager.Instance.downKey) && !isGround)
            {
				animator.SetTrigger("setAttackDown");
            }
            else
            {
				animator.SetTrigger("setAttackLR");
            }
        }
    }

    #endregion
	
	#region Sprint

    void Sprint()
    {
		if (!inputEnable)
        {
            return;
        }
		
		if(isCanSprint && Input.GetKeyDown(InputManager.Instance.sprintKey))
		{
			if(isClimb) //如果在爬墙则需要调头
            {
                transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
            }
            StartCoroutine(SprintMove());
            if (isShadowSprint)
            {
                if (isGround)
                {
                    animator.SetTrigger("setShadowSprintGround");//播放冲刺动画
                }
                else
                {
                    animator.SetTrigger("setShadowSprintFly");//播放冲刺动画
                }
            }
            else
            {
                animator.SetTrigger("setSprint");//播放冲刺动画
            }

            isCanSprint = false;
        }
    }
	
	IEnumerator SprintMove()
    {
        inputEnable = false;
        gravityEnable = false;
        moveSpeed.y = 0;

        moveSpeed.x = -15 * transform.localScale.x; 

        yield return new WaitForSeconds(sprintTime);

        inputEnable = true;
        gravityEnable = true;
    }

    #endregion

    #region SuperSprint

    void SuperSprint()
    {

    }

    #endregion

    #region NextFrameMove

    void NextFrameMove()
    {
        if(isClimb)
        {
            animator.SetTrigger("setClimb");
        }
        Vector2 moveDistance = moveSpeed * Time.deltaTime;

        if (moveDistance.x != 0) //左右有移动
        {
            RaycastHit2D lRHit2D = Physics2D.BoxCast(transform.position, boxSize, 0, moveDistance.x > 0 ? Vector3.right : Vector3.left, 5.0f, playerLayerMask);   //发射盒子射线
            if (lRHit2D.collider != null)//如果当前方向上有碰撞体
            {
                DrawBoxLine(lRHit2D.point, boxSize, 3.0f);  //显示盒子射线

                float tempXVaule = (float)Math.Round(lRHit2D.point.x, 1);                   //取X轴方向的数值，并保留1位小数精度。防止由于精度产生鬼畜行为
                Vector2 colliderPoint = new Vector2(tempXVaule, transform.position.y); //调整碰撞点
                float tempDistance = Vector3.Distance(colliderPoint, transform.position);   //计算玩家与碰撞点的位置
                if(tempDistance > boxSize.x * 0.5f + minDistance)   //在不可以移动的范围外
                {
                    transform.position += new Vector3(moveDistance.x, 0, 0); //说明此时还能进行正常移动，不需要进行修正
                    if (isClimb)        //如果左右方向没有碰撞体了，退出爬墙状态
                    {
                        isClimb = false;
                        animator.ResetTrigger("setClimb"); //重置触发器  退出
                        animator.SetTrigger("setClimbToJumpDown");
                    }
                }
                else //修正移动距离
                {
                    float tempX = tempXVaule + (boxSize.x * 0.5f + minDistance - 0.05f) * transform.localScale.x;//新的X轴的位置,多加上0.05f的修正距离，防止出现由于精度问题产生的鬼畜行为

                    transform.position = new Vector3(tempX, transform.position.y, 0);//修改玩家的位置
                    if (lRHit2D.collider.CompareTag("Untagged"))    //如果左右是墙或者天花板
                    {
                        ClimbFunc(transform.position); //检测当前是否能够进入爬墙状态
                        animator.ResetTrigger("setClimbToJumpDown");
                    }
                }
            }
            else
            {
                transform.position += new Vector3(moveDistance.x, 0, 0);
                if (isClimb)
                {
                    isClimb = false;
                    animator.SetTrigger("setClimbToJumpDown");
                    animator.ResetTrigger("setClimb"); //重置触发器  退出
                }
            }
        }
        else
        {
            if (isClimb)    //当左右速度无值时且处于爬墙状态时
            {
                ExitClimbFunc();
            }
        }

        if(moveDistance.y != 0) //上下移动
        {
            RaycastHit2D uDHit2D = Physics2D.BoxCast(transform.position, boxSize, 0, moveDistance.y > 0?Vector3.up:Vector3.down, 5.0f, playerLayerMask);
            if (uDHit2D.collider != null)
            {
                float tempYVaule = (float)Math.Round(uDHit2D.point.y, 1);
                Vector3 colliderPoint = new Vector3(transform.position.x, tempYVaule);
                float tempDistance = Vector3.Distance(transform.position, colliderPoint);

                if (tempDistance > (boxSize.y * 0.5f + minDistance))
                {
                    float tempY = 0;
                    float nextY = transform.position.y + moveDistance.y;
                    if (moveDistance.y > 0)
                    {
                        tempY = tempYVaule - boxSize.y * 0.5f - minDistance;
                        if (nextY > tempY)
                        {
                            transform.position = new Vector3(transform.position.x, tempY + 0.1f, 0);
                        }
                        else
                        {
                            transform.position += new Vector3(0, moveDistance.y, 0);
                        }
                    }
                    else
                    {
                        tempY = tempYVaule + boxSize.y * 0.5f + minDistance;
                        if (nextY < tempY)
                        {
                            transform.position = new Vector3(transform.position.x, tempY - 0.1f, 0); //上下方向多减少0.1f的修正距离，防止鬼畜
                        }
                        else
                        {
                            transform.position += new Vector3(0, moveDistance.y, 0);
                        }
                    }
                    isGround = false;   //更新在地面的bool值
                }
                else
                {
                    float tempY = 0;
                    if (moveDistance.y > 0)//如果是朝上方向移动，且距离小于规定距离，就说明玩家头上碰到了物体，反之同理。
                    {
                        tempY = uDHit2D.point.y - boxSize.y * 0.5f - minDistance + 0.05f;
                        isGround = false;
                        Debug.Log("头上碰到了物体");
                    }
                    else
                    {
                        tempY = uDHit2D.point.y + boxSize.y * 0.5f + minDistance - 0.05f;
                        Debug.Log("着地");
                        isGround = true;
                    }
                    moveSpeed.y = 0;
                    transform.position = new Vector3(transform.position.x, tempY, 0);
                }
            }
            else
            {
                isGround = false;
                transform.position += new Vector3(0, moveDistance.y, 0);
            }
        }
        else
        {
            isGround = CheckIsGround();//更新在地面的bool值
        }
    }

    /// <summary>
    /// 显示盒子射线
    /// </summary>
    public void DrawBoxLine(Vector3 point, Vector2 size, float time = 0)
    {
        float x, y;
        x = point.x;
        y = point.y;
        float m, n;
        m = size.x;
        n = size.y;

        Vector3 point1, point2, point3, point4;
        point1 = new Vector3(x - m * 0.5f, y + n * 0.5f, 0);
        point2 = new Vector3(x + m * 0.5f, y + n * 0.5f, 0);
        point3 = new Vector3(x + m * 0.5f, y - n * 0.5f, 0);
        point4 = new Vector3(x - m * 0.5f, y - n * 0.5f, 0);

        Debug.DrawLine(point1, point2, Color.red, time);
        Debug.DrawLine(point2, point3, Color.red, time);
        Debug.DrawLine(point3, point4, Color.red, time);
        Debug.DrawLine(point4, point1, Color.red, time);
    }

    /// <summary>
    /// 进入爬墙的函数
    /// </summary>
    public void ClimbFunc(Vector3 rayPoint)
    {
        //设定碰到墙 且  从碰撞点往下 玩家碰撞盒子高度内  没有碰撞体  就可进入碰撞状态。
        RaycastHit2D hit2D = Physics2D.BoxCast(rayPoint, boxSize, 0, Vector2.down, boxSize.y, playerLayerMask);
        if (hit2D.collider != null)
        {
            Debug.Log("无法进入爬墙状态  " + hit2D.collider.name);
        }
        else
        {
            //如果上方是异形碰撞体，那么就无法进入爬墙状态
            hit2D = Physics2D.BoxCast(rayPoint, boxSize, 0, Vector2.up, boxSize.y * 0.8f, playerLayerMask);
            if (hit2D.collider == null)
            {
                animator.SetTrigger("setClimb");//动画切换
                isClimb = true;
                isCanSprint = true; //爬墙状态，冲刺重置
            }
        }
    }

    /// <summary>
    /// 退出爬墙状态检测
    /// </summary>
    public void ExitClimbFunc()
    {
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, Vector2.left * transform.localScale.x, boxSize.x);

        if (hit2D.collider == null)//下落到左右没有墙壁时
        {
            isClimb = false;
            animator.SetTrigger("setClimbToJumpDown");
            animator.ResetTrigger("setClimb"); //重置触发器  退出
        }
    }

    /// <summary>
    /// 检测是否在地面
    /// </summary>
    /// <returns></returns>
    public bool CheckIsGround()
    {
        float aryDistance = boxSize.y * 0.5f + 0.1f;
        RaycastHit2D hit2D = Physics2D.BoxCast(transform.position, boxSize, 0, Vector2.down, 5f, playerLayerMask);
        Debug.DrawLine(transform.position, transform.position + Vector3.down * aryDistance, Color.red, 6.0f);
        if (hit2D.collider != null)
        {

            float tempDistance = Vector3.Distance(transform.position, hit2D.point);
            if (tempDistance > (boxSize.y * 0.5f + minDistance))
            {
                //transform.position += new Vector3(0, moveDistance.y, 0);
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    #endregion
}
