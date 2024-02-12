using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace BearK.WayFinding
{
    public class EntityHook : MonoBehaviour
    {
        private FlowEntity _entity;

        public void Awake()
        {
            _entity = new FlowEntity(transform.position);
            _entity.InjectAgent(GetComponent<NavMeshAgent>());
        }

        public void Start()
        {
            
        }

        public FlowEntity GetEntity()
        {
            return _entity;
        }
    }
}
