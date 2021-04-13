using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TankGame
{


    public class Bullet : MonoBehaviour
    {
        public GameObject Explosion;
        public float Speed = 100f;
        public float LifeTime = 2f;

        private float initTime;

        // Start is called before the first frame update
        void Start()
        {
            initTime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            transform.Translate(0, 0, Speed * Time.deltaTime, Space.Self);

            if (Time.time - initTime > LifeTime)
                Explode();
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    Explode();
        //}

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(Tags.Player))
                return;
            Debug.Log("collision" + collision);
            Explode();
        }

        private void Explode()
        {
            Debug.Log(transform.position + "  " + transform.rotation);
            if (Explosion)
                Instantiate(Explosion, transform.position, transform.rotation);
            Destroy(this.gameObject);
        }

    }

}