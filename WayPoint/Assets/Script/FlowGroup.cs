/****************************************************
	Writer：BearK
	E-Mail：likeqinhz@vip.qq.com 
	Date：2024/02/08
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BearK.WayFinding
{
    public class FlowGroup
    {
        /// <summary>
        /// 群体规模
        /// </summary>
        private int groupSize;
        /// <summary>
        /// 尝试次数
        /// </summary>
        private int tryTime;
        /// <summary>
        /// 移动步长
        /// </summary>
        private int step;
        /// <summary>
        /// 拥挤因子
        /// </summary>
        private float crowdFactor;
        /// <summary>
        /// 视野范围
        /// </summary>
        private float visual;

        /// <summary>
        /// 个体群
        /// </summary>
        //private FlowEntity[] entities;
        private List<FlowEntity> entities;
        /// <summary>
        /// 最佳个体
        /// </summary>
        //private FlowEntity bestEntity;
        /// <summary>
        /// 预存结果
        /// </summary>
        private FlowEntity[] nextEntities;
        /// <summary>
        /// 目标群
        /// </summary>
        private List<Transform> FindPoints;

        /// <summary>
        /// 检测范围内群体数量
        /// </summary>
        public int scopeNum;

        public const int MAX_PREY_OFFSET = 1;
        public const int MAX_SWARM_OFFSET = 1;
        public const int MAX_FOLLOW_OFFSET = 1;

        public FlowGroup() { }

        public FlowGroup(int groupSize, int tryTime, int step, float crowdFactor, float visual, List<Transform> findPoints)
        {
            this.groupSize = groupSize;
            this.tryTime = tryTime;
            this.step = step;
            this.crowdFactor = crowdFactor;
            this.visual = visual;
            this.FindPoints = findPoints;

            this.entities = new List<FlowEntity>();
            this.nextEntities = new FlowEntity[3];
            //this.index = 0;

            Init();
        }

        private void Init()
        {
            try
            {
                var hooks =  GameObject.Find("EntityRoot").GetComponentsInChildren<EntityHook>();
                foreach (var hook in hooks)
                {
                    var entity = hook.GetEntity();
                    if(entity != null)
                    {
                        entity.InjectVisual(visual);
                        entity.ComputeFit();
                        entities.Add(entity);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void Navigate(int iterationNum)
        {
            int count = 0;
            while (count++ < iterationNum)
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    CheckStop(entities[i]);
                    if (entities[i].IsStop) continue;

                    Swarm(i);
                    Follow(i);
                    Prey(i);
                    Bulletin(i);
                    entities[i].Move();
                    for (int j = 0; j < nextEntities.Length; j++)
                    {
                        nextEntities[j] = null;
                    }
                }
            }
        }

        private void CheckStop(FlowEntity entity)
        {
            if (entity.IsStop) return;
            if (entity.GetTarget())
            {
                if (Vector3.Distance(entity.Agent.transform.position, entity.GetTarget().position) < 0.8f)
                { 
                    entity.IsStop = true;
                    Debug.Log("Find Target Point Sucess");
                    return;
                }
            }
            entity.IsStop = false;
        }

        #region
        private void Prey(int index)
        {
            var curEntity = entities[index];
            for (int i = 0; i < tryTime; i++)
            {
                Vector3 random = UnityEngine.Random.insideUnitSphere;
                random.y = 0;
                Vector3 newPos = curEntity.Offset + curEntity.Visual * random;

                //当前存在目标
                if (curEntity.CheckTarget())
                {
                    FlowEntity newEntity = new FlowEntity(newPos);
                    newEntity.InjectVisual(curEntity.Visual);
                    newEntity.InjectAgent(curEntity.Agent);
                    newEntity.RegisterTarget(curEntity.GetTarget());
                    newEntity.ComputeFit(curEntity.GetTarget().position);
                    if (newEntity.Fit < curEntity.Fit)
                    {
                        float dis = curEntity.Distance(newEntity);
                        Vector3 nextRandom = UnityEngine.Random.insideUnitSphere * MAX_PREY_OFFSET;
                        nextRandom.y = 0;
                        Vector3 nextPos = curEntity.Offset + (newEntity.Offset - curEntity.Offset) * step / dis;//.Mult(nextRandom)
                        nextEntities[0] = newEntity;
                        nextEntities[0].InjectVisual(curEntity.Visual);
                        nextEntities[0].RegisterTarget(curEntity.GetTarget());
                        nextEntities[0].InjectAgent(curEntity.Agent);
                        nextEntities[0].ComputeFit(nextEntities[0].GetTarget().position);
                        return;
                    }
                }
                else 
                {
                    FlowEntity newEntity = new FlowEntity(newPos);
                    newEntity.InjectVisual(curEntity.Visual);
                    newEntity.ComputeFit();
                    //查找目标点
                    Transform nearestPoint = GetNearestPoint(newEntity);
                    if (nearestPoint != null)
                    {
                        newEntity.RegisterTarget(nearestPoint);
                        newEntity.InjectAgent(curEntity.Agent);
                        newEntity.ComputeFit(newEntity.GetTarget().position);
                        nextEntities[0] = newEntity;

                        Debug.Log("Find Target Point, position: " + nearestPoint.position);
                        return;
                    }
                }

            }

            //在N次迭代下没有找到更优点，则赋予随机点
            nextEntities[0] = RandomShift(index);
        }

        private void Swarm(int index)
        {
            FlowEntity curEntity = entities[index];
            Vector3 center = Vector3.zero;
            List<FlowEntity> scopes = GetScope(index);
            if (scopes.Count > 0)
            {
                foreach (var entity in scopes)
                {
                    if(entity.CheckTarget())
                        center += entity.Offset;
                }
                center /= scopes.Count;

                FlowEntity centerEntity = new FlowEntity(center);
                centerEntity.InjectVisual(curEntity.Visual);
                //无需关心具体目标和开销
                centerEntity.ComputeFit();

                List<FlowEntity> newScopes = GetScope(centerEntity);
                //当前对象没有目标时  向允许新加入且个体含量更高的区域移动
                if (!curEntity.CheckTarget() 
                    && scopes.Count < newScopes.Count 
                    && newScopes.Count < crowdFactor)
                {
                    float dis = curEntity.Distance(centerEntity);
                    Vector3 nextRandom = UnityEngine.Random.insideUnitSphere * MAX_SWARM_OFFSET;
                    Vector3 nextPos = curEntity.Offset + (centerEntity.Offset - curEntity.Offset) * step / dis;//.Mult(nextRandom)
                    nextEntities[1] = new FlowEntity(nextPos);
                    nextEntities[1].InjectVisual(curEntity.Visual);
                    //无目标 无开销值
                    nextEntities[1].ComputeFit();
                    return;
                }
            }
        }

        private void Follow(int index)
        {
            FlowEntity curEntity = entities[index];
            FlowEntity minEntity = null;
            List<FlowEntity> scopes = GetScope(index);
            if (scopes.Count > 0)
            {
                minEntity = null;
                int minIndex = -1;
                for (int i = 1; i < scopes.Count; i++)
                {
                    if (scopes[i].CheckTarget())
                    {
                        if (minIndex == -1 || scopes[i].Fit < minEntity.Fit)
                        { 
                            minIndex = i;
                            minEntity = scopes[i];
                        }
                    }
                }

                //存在最佳体
                if (minIndex != -1)
                {
                    float minDis = Vector3.Distance(curEntity.Offset, minEntity.Offset);
                    if ((curEntity.CheckTarget() && minDis + minEntity.Fit < curEntity.Fit)
                        || !curEntity.CheckTarget())
                    {
                        List<FlowEntity> newScopes = GetScope(minIndex);
                        if (newScopes.Count > 0)
                        {
                            //拥挤程度允许加入
                            if (newScopes.Count < crowdFactor)
                            {
                                float dis = curEntity.Distance(minEntity);
                                Vector3 nextRandom = UnityEngine.Random.insideUnitSphere * MAX_FOLLOW_OFFSET;
                                Vector3 nextPos = curEntity.Offset + (minEntity.Offset - curEntity.Offset) * step / dis;//.Mult(nextRandom)
                                nextEntities[2] = new FlowEntity(nextPos);
                                nextEntities[2].InjectVisual(curEntity.Visual);
                                nextEntities[2].RegisterTarget(minEntity.GetTarget());
                                nextEntities[2].InjectAgent(curEntity.Agent);
                                nextEntities[2].ComputeFit(nextEntities[2].GetTarget().position);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void Bulletin(int index)
        {
            try
            {
                FlowEntity curEntity = entities[index];
                FlowEntity resultEntity = null;
                foreach (var entity in nextEntities)
                {
                    if (entity == null) continue;
                    if (resultEntity == null)
                    {
                        resultEntity = entity;
                        continue;
                    }
                    else
                    {
                        if (entity.CheckTarget())
                        {
                            if (entity.Fit < resultEntity.Fit || resultEntity.Fit == 0)
                            {
                                resultEntity = entity;
                                continue;
                            }
                        }
                    }
                }
                if (resultEntity == null)
                {
                    Debug.LogError("Result Entity is Null: " + index);
                    return;
                }

                {
                    curEntity.InjectOffset(resultEntity.Offset);
                    if (resultEntity.CheckTarget())
                    {
                        curEntity.RegisterTarget(resultEntity.GetTarget());
                        curEntity.ComputeFit(curEntity.GetTarget().position);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
         }

        private FlowEntity RandomShift(int index)
        {
            FlowEntity curEntity = entities[index];
            Vector3 nextRandom = UnityEngine.Random.insideUnitSphere;
            nextRandom.y = 0;
            Vector3 nextPos = curEntity.Offset + step * nextRandom;
            FlowEntity result = new FlowEntity(nextPos);
            result.InjectVisual(curEntity.Visual);
            result.ComputeFit();
            return result;
        }

        private List<FlowEntity> GetScope(int index)
        {
            FlowEntity entity = entities[index];
            return GetScope(entity);
        }

        private List<FlowEntity> GetScope(FlowEntity entity)
        {
            List<FlowEntity> sets = new List<FlowEntity>();
            for (int i = 0; i < entities.Count; i++)
            {
                if (entity != entities[i] && entity.Distance(entities[i]) < entity.Visual)
                {
                    sets.Add(entity);
                }
            }

            return sets;
        }

        #endregion

        private Transform GetNearestPoint(FlowEntity entity)
        {
            float minDis = float.MaxValue;
            Transform minTran = null;
            foreach (var tran in FindPoints)
            {
                float dis = Vector3.Distance(entity.Offset, tran.position);
                if (dis < entity.Visual && dis < minDis)
                {
                    minDis = dis;
                    minTran = tran;
                }
            }
            return minTran;
        }
    }

    public static class FlowGroupTool
    {
        public static Vector3 Mult(this Vector3 vec1, Vector3 vec2)
        {
            vec1.x *= vec2.x;
            vec1.y *= vec2.y;
            vec1.z *= vec2.z;
            return vec1;
        }
    }
}
