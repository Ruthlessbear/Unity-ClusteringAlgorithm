/****************************************************
	Writer：BearK
	E-Mail：likeqinhz@vip.qq.com 
	Date：2024/02/08
*****************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace BearK.WayFinding
{
    /// <summary>
    /// 流动对象
    /// </summary>
    public class FlowEntity
    {
        public const int MAX_FLOW_OFFSET = 256;

        public Vector3 Offset;           //偏移
        public float Fit;                    //浓度
        public float Visual;                  //视野
        public Transform Target;       //目标
        public bool IsTarget;            //是否拥有目标对象
        public NavMeshAgent Agent;

        private const float MoveScale = 2f;

        //Temp
        public bool IsStop = false;

        public FlowEntity(Vector3 pos)
        {
            this.Visual = 0;
            this.Fit = 0;
            this.Offset = pos;
        }

        public float Distance(FlowEntity targetEntity)
        {
            return Vector3.Distance(this.Offset, targetEntity.Offset);
        }

        public void ComputeFit()
        {
            this.Fit = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="des"></param>
        public void ComputeFit(Vector3 des)
        {
            try
            {
                Vector3[] corners = Agent.GetCorners(des);
                //这里将第一个拐点替换为存储的偏移值
                corners[0] = Offset;
                //根据拐点计算路径开销 （假设成本都一致，实际需要多一步成本计算）
                float cost = 0;
                Vector3 n1, n2;
                for (int i = 0; i < corners.Length - 1; i++)
                {
                    n1 = corners[i];
                    n2 = corners[i + 1];
                    cost += Vector3.Distance(n1, n2);
                }
                this.Fit = cost;
            }
            catch
            {
                Debug.Log("Compute Fit Faild, Check NavmeshAgent.");
            }
        }

        /// <summary>
        /// 注入视野范围
        /// </summary>
        public void InjectVisual(float visual)
        {
            this.Visual = visual;
        }

        /// <summary>
        /// 注入NavAgent
        /// </summary>
        /// <param name="agent"></param>
        public void InjectAgent(NavMeshAgent agent)
        {
            this.Agent = agent;
        }

        /// <summary>
        /// 注入偏移量
        /// </summary>
        /// <param name="offset"></param>
        public void InjectOffset(Vector3 offset)
        {
            this.Offset = offset;
        }

        /// <summary>
        /// 获取当前目标
        /// </summary>
        /// <returns></returns>
        public Transform GetTarget()
        {
            return Target;
        }

        public bool CheckTarget()
        {
            return this.IsTarget;
        }

        public void RegisterTarget(Transform target)
        {
            this.Target = target;
            IsTarget = true;
        }

        public void LogoutTarget()
        {
            this.Target = null;
            IsTarget = false;
        }

        public void Move()
        {
            Agent.destination = this.Offset;
            Agent.speed = Vector3.Distance(Agent.nextPosition, Agent.destination) / MoveScale;
        }
    }
}
