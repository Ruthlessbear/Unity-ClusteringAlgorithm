using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace BearK.WayFinding
{
    class GameManager : MonoBehaviour
    {
        [Flags]
        public enum PointFlags
        { 
            NONE,
            A1 = 1,
            A2 = 2,
            A3 = 4,
            A4 = 8,
            B1 = 16,
            B2 = 32,
            B3 = 64,
            B4 = 128,
            C1 = 256,
            C2 = 512,
            C3 = 1024,
            C4 = 2048
        }
       
        public PointFlags LoadFlags;

        public GameObject EntityPrefab;
        public GameObject Root;
        public GameObject NavPoint;
        public int SinglePointNum = 10;
        public float LoadRange = 2.0f;
        public List<Transform> LoadPoint;
        public List<Transform> FindPoint;

        //Runtime
        private List<GameObject> runEntities = new List<GameObject>();
        private FlowGroup group;

        private void Awake()
        {
            
        }

        private void Start()
        {
            SortLoadPoint();
            Initialize();
            InitGroup();
        }

        private void Update()
        {
            
        }

        private void SortLoadPoint()
        {
            LoadPoint.Sort((x, y)=> {
                int xIndex = int.Parse(x.name);
                int yIndex = int.Parse(y.name);
                return xIndex < yIndex ? -1 : 1;
            });

            FindPoint.Sort((x, y)=>{
                int xIndex = int.Parse(x.name);
                int yIndex = int.Parse(y.name);
                return xIndex < yIndex ? -1 : 1;
            });
        }

        private void Initialize()
        {
            //在指定点位随机生成对象
            var flags = (PointFlags[])Enum.GetValues(typeof(PointFlags));
            foreach (var flag in flags)
            {
                if (flag.Equals(PointFlags.NONE)) continue;
                if (LoadFlags.HasFlag(flag))
                {
                    for (int i = 0; i < SinglePointNum; i++)
                    {
                        var entity = LoadEntity(flag);
                        runEntities.Add(entity);
                    }
                }
            }
        }

        private void InitGroup()
        {
            group = new FlowGroup(20, 5, 1, 20, 2f, FindPoint);
            StartCoroutine(UpdateGroup());

            //Test
            //int i = 0;
            //foreach (var entity in runEntities)
            //{
            //    NavMeshPath path = new NavMeshPath();
            //    NavMeshAgent agent = entity.GetComponent<NavMeshAgent>();
            //    agent.CalculatePath(NavPoint.transform.position, path);
            //    agent.destination = NavPoint.transform.position;

            //    float cost = 0;
            //    Vector3 n1 = entity.transform.position;
            //    Vector3 n2;
            //    foreach (var corner in path.corners)
            //    {
            //        n2 = corner;
            //        cost += Vector3.Distance(n1, n2);
            //        //Debug.DrawLine(n1, n2, Color.blue);
            //        n1 = corner;
            //    }
            //    Debug.Log(string.Format("{0} : {1}", ++i, cost));
            //}
        }

        private  IEnumerator UpdateGroup()
        {
            while (true)
            {
                group.Navigate(1);

                yield return new WaitForSeconds(2f);
            }
        }

        private GameObject LoadEntity(PointFlags flag)
        {
            int index = 0;
            switch (flag)
            {
                case PointFlags.A1:
                case PointFlags.A2:
                case PointFlags.A3:
                case PointFlags.A4:
                    break;
                case PointFlags.B1:
                case PointFlags.B2:
                case PointFlags.B3:
                case PointFlags.B4:
                    index += 4;
                    break;
                case PointFlags.C1:
                case PointFlags.C2:
                case PointFlags.C3:
                case PointFlags.C4:
                    index += 8;
                    break;
            }
            int add = int.Parse(flag.ToString().Substring(1, 1));
            index += add - 1;

            Vector3 random = UnityEngine.Random.insideUnitSphere;
            random.y = 0;
            Vector3 originalPos = LoadPoint[index].position;
            Vector3 resultPos = originalPos + random * LoadRange;

            var obj = GameObject.Instantiate(EntityPrefab, resultPos, Quaternion.identity, Root.transform);
            obj.AddComponent<EntityHook>();

            return obj;
        }
    }
}
