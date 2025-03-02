using UnityEngine;
    
namespace CustomPhysic
{
    public class GlobalParameters : MonoBehaviour
    {
        public static GlobalParameters instance;

        public Vector3 Gravity = new Vector3(0, -9.81f, 0);

        private void Awake()
        {
            instance = this;
        }
    }
}
