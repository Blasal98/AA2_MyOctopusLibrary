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

        void update_ccd()
        {
                
            for (int i = 0; i < _tentacles.Length; i++) //recorrem cada tentacle
            {
                bool done = false;
                float rotationAngle;
                float cos;
                float error = 0.1f;
                int tries = 0;

                while (!done && tries < 10) //si compleix el nostre criteri d'error o si supera limit de calculs (10)
                {
                    for (int j = _tentacles[i].Bones.Length - 1; j >= 0; j--) //recorrem cada bone desde l'ultim
                    {

                        Vector3 E_R = _tentacles[i].EndEffector[0].transform.position - _tentacles[i].Bones[j].transform.position; //vector de posicio de joint a endeffector
                        Vector3 T_R = _randomTargets[i].transform.position - _tentacles[i].Bones[j].transform.position; //vector de posicio de joint al target

                        if (E_R.magnitude * T_R.magnitude <= 0.001f) //evitem valors petits per no tenir valors massa grans
                            cos = 1;
                        
                        else //sino u calculem com u fariem normalment -> aillar cos de la formula de dot product
                            cos = Vector3.Dot(E_R, T_R) / (E_R.magnitude * T_R.magnitude);

                        rotationAngle = Mathf.Acos(Mathf.Max(-1, Mathf.Min(1, cos))); //garanteix que acos operi amb argument entre -1 i 1
                        rotationAngle = (float)angleBetweenPIAndMinusPI(rotationAngle) * Mathf.Rad2Deg; //acotar l'angle entre -PI i PI ->anar pel cami curt-> i posaro en degrees

                        _tentacles[i].Bones[j].Rotate(Vector3.Cross(E_R,T_R).normalized,rotationAngle,Space.World); //aplicar rotacio
                                
                    }
                    tries++; //controlem quantes iteracions portem per no superar el limit de calculs establert (10)

                    //calculem distancia entre endeffector i target per a cada component del eix de coordenades
                    float x = Mathf.Abs(_tentacles[i].EndEffector[0].transform.position.x - _randomTargets[i].transform.position.x); 
                    float y = Mathf.Abs(_tentacles[i].EndEffector[0].transform.position.y - _randomTargets[i].transform.position.y);
                    float z = Mathf.Abs(_tentacles[i].EndEffector[0].transform.position.z - _randomTargets[i].transform.position.z);

                    if (x < error && y < error && z < error) //si la distancia a cada component es menor que el nostre error hem acabat
                        done = true;
                }
            }

            
        }

        double angleBetweenPIAndMinusPI(double angle)
        {
            angle = angle % (2.0 * Mathf.PI);
            if (angle < -Mathf.PI)
                angle += 2.0 * Mathf.PI;
            else if (angle > Mathf.PI)
                angle -= 2.0 * Mathf.PI;
            return angle;
        }

        #endregion






    }
}
