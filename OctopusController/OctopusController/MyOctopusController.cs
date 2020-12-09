using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController 
    {
        
        MyTentacleController[] _tentacles =new  MyTentacleController[4];
        Transform _currentRegion;
        Transform _target;

        Transform[] _randomTargets;// = new Transform[4];


        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        #region public methods
        //DO NOT CHANGE THE PUBLIC METHODS!!

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin {  set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }

        bool aux = false;

        public void TestLogging(string objectName)
        {

           
            Debug.Log("hello, I am initializing my Octopus Controller in object / Aguilo"+objectName);

            
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            // foreach (Transform t in tentacleRoots)
            for(int i = 0;  i  < tentacleRoots.Length; i++)
            {

                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i],TentacleMode.TENTACLE);
                //TODO: initialize any variables needed in ccd
                
            }

            _randomTargets = randomTargets;
            //TODO: use the regions however you need to make sure each tentacle stays in its region

        }

              
        public void NotifyTarget(Transform target, Transform region)
        {
            _currentRegion = region;
            _target = target;
        }

        public void NotifyShoot() {
            //TODO. what happens here?
            Debug.Log("Shoot");
        }


        public void UpdateTentacles()
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            update_ccd();
            
        }




        #endregion


        #region private and internal methods
        //todo: add here anything that you need

        void update_ccd()
        {

            //_tentacles[0].Bones[30].transform.Rotate(new Vector3(1,0,1).normalized, 1);
            if (!aux)
            {
                
                //aux = true;
                for (int i = 0; i < _tentacles.Length; i++) //recorrem cada tentacle
                {
                    bool done = false;
                    float[] theta = new float[_tentacles[i].Bones.Length];
                    float[] sin = new float[_tentacles[i].Bones.Length];
                    float[] cos = new float[_tentacles[i].Bones.Length];
                    float epsilon = 0.1f;
                    int tries = 0;
                    //_tentacles[i].Bones[49].transform.position = _randomTargets[i].position;
                    while (!done && tries < 10)
                    {
                        
                        for (int j = _tentacles[i].Bones.Length - 1; j >= 0; j--) //recorrem cada bone desde l'ultim
                        {

                            Vector3 E_R = _tentacles[i].EndEffector[0].transform.position - _tentacles[i].Bones[j].transform.position;
                            Vector3 T_R = _randomTargets[i].transform.position - _tentacles[i].Bones[j].transform.position;

                            if (E_R.magnitude * T_R.magnitude <= 0.001f)
                            {
                                // cos component will be 1 and sin will be 0
                                cos[j] = 1;
                                sin[j] = 0;
                            }
                            else
                            {
                                // find the components using dot and cross product
                                cos[j] = Vector3.Dot(E_R, T_R) / (E_R.magnitude * T_R.magnitude);
                                sin[j] = (Vector3.Cross(E_R, T_R)).magnitude / (E_R.magnitude * T_R.magnitude);

                            }
                            theta[j] = Mathf.Acos(Mathf.Max(-1, Mathf.Min(1, cos[j])));
                            if (sin[j] < 0.0f)
                                theta[j] = -theta[j];
                            theta[j] = (float)SimpleAngle(theta[j]) * Mathf.Rad2Deg;

                            //Quaternion q = Quaternion.FromToRotation(E_R.normalized, T_R.normalized);
                            _tentacles[i].Bones[j].Rotate(Vector3.Cross(E_R,T_R).normalized,theta[j],Space.World);
                                
                        }
                        tries++;
                        float x = Mathf.Abs(_tentacles[i].EndEffector[0].transform.position.x - _randomTargets[i].transform.position.x);
                        float y = Mathf.Abs(_tentacles[i].EndEffector[0].transform.position.y - _randomTargets[i].transform.position.y);
                        float z = Mathf.Abs(_tentacles[i].EndEffector[0].transform.position.z - _randomTargets[i].transform.position.z);

                        // if target is within reach (within epsilon) then the process is done
                        if (x < epsilon && y < epsilon && z < epsilon)
                        {
                            done = true;
                        }
                        // if it isn't, then the process should be repeated
                        else
                        {
                            done = false;
                            
                        }
                        
                    }
                }

            }
        }

        double SimpleAngle(double theta)
        {
            theta = theta % (2.0 * Mathf.PI);
            if (theta < -Mathf.PI)
                theta += 2.0 * Mathf.PI;
            else if (theta > Mathf.PI)
                theta -= 2.0 * Mathf.PI;
            return theta;
        }

        #endregion






    }
}
