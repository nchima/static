using UnityEngine;
using System.Collections;

public class PlayerBulletExperimental : MonoBehaviour
{
    // USED FOR BEING DELETED

    [SerializeField] float deleteTime = 0.25f;   // How long this bullet lasts after hitting something before being deleted.
    private float timeSinceHit;    // How long this bullet has existed thus far.
    [SerializeField] private bool readyToBeDeleted;  // Whether this bullet has already hit something or not.


    // USED FOR MOVING

    private bool isActive;    // Whether this bullet is being fired.

    private float speed = 0f;   // How many seconds it takes me to move from the position I was fired from to my end position.
    private float lerpTime = 0f;    // Used to calculate current position.

    private Vector3 originalGunPos; // The position of the gun at the time that I was fired.
    private Vector3 endPoint;   // The position I am being fired towards.
    private Vector3 bulletFrontPos // The point at the very front of this bullet.
    {
        get
        {
            return transform.position + transform.up * (worldLength * 0.5f);
        }
    }   
    private Vector3 bulletBackPos // The point at the very back of this bullet.
    {
        get
        {
            return transform.position + (-transform.up * worldLength) * 0.5f;
        }
    }   
    private float worldLength
    {
        get
        {
            return transform.localScale.y * 2f;
        }
    }
    private float maxLength;
    private Vector3 nextFrontPosition;  // The next point that I'll get to.
    private Vector3 nextBackPosition;


    // USED FOR HITTING THINGS

    [SerializeField] bool hitSomething = false;
    [SerializeField] GameObject strikeParticles;    // The particles which appear when I strike any object.


    // REFERENCEE
    GameManager gameManager;
    Gun gun;


    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        gun = FindObjectOfType<Gun>();
    }


    private void Update()
    {
        if (readyToBeDeleted)
        {
            WaitToBeDeleted();
        }

        else
        {
            if (isActive)
            {
                HandleMovement();
                //MoveForward();
            }
        }
    }


    private void HandleMovement()
    {
        // First get a new front position.
        GetNewLerpTime();
        nextFrontPosition = Vector3.Lerp(originalGunPos, endPoint, lerpTime);
        nextBackPosition = bulletBackPos;

        // See if this next position would result in a hit.
        TestForHit(bulletBackPos, nextFrontPosition);

        // If I've reached my maximum range, just pretend I hit something.
        if (lerpTime >= 1)
        {
            //Debug.Log("Reached maximum range.");
            hitSomething = true;
        }

        if (hitSomething)
        {
            //Debug.Log("Hit something later on");
            Debug.DrawRay(transform.position, Vector3.up, Color.red, 1f);
            timeSinceHit = 0f;
            readyToBeDeleted = true;
        }

        else
        {
            nextFrontPosition = nextFrontPosition + transform.up * (speed * Time.deltaTime);
        }

        nextBackPosition = nextFrontPosition + (-transform.up * (maxLength));
        SetTransformByEndPoints(nextBackPosition, nextFrontPosition);
    }


    private void TestForHit(Vector3 start, Vector3 end)
    {
        // Spherecast forward to my next position.
        RaycastHit hit;
        //Debug.Log("End point: " + end + ", lerp time: " + lerpTime);
        //Debug.Log("Start point: " + start);
        Debug.DrawLine(originalGunPos, endPoint, Color.cyan);
        Debug.DrawLine(start, end, Color.red);
        if (Physics.SphereCast(start, transform.localScale.x, transform.up, out hit, Vector3.Distance(start, end), (1 << 8) | (1 << 13) | (1 << 14)))
        {
            // If the spherecast hit anything, move so that my tip is just touching the position of the hit.
            nextFrontPosition = hit.point;

            // Show the particle effects for hitting something.
            Instantiate(strikeParticles, hit.point, Quaternion.identity);

            // If I hit an enemy, do the appropriate things.
            if (hit.transform.tag == "Enemy")
            {
                // Tell the enemy it was hurt.
                //hit.transform.GetComponent<Enemy>().HP -= Random.Range(gun.bulletDamageMin, gun.bulletDamageMax);

                // Tell the score controller that the player hit an enemy with a bullet.
                gameManager.BulletHit();
            }

            else if (hit.transform.name.Contains("Homing Shot"))
            {
                hit.transform.GetComponent<HomingShot>().Detonate();
            }

            timeSinceHit = 0f;
            hitSomething = true;
        }

        // If the spherecast didn't hit anything, move to the position it spherecasted to.
        else
        {
            // Make sure the length of the bullet is not greater than the distance that it's travelled.
            //if (transform.localScale.y > Vector3.Distance(originalGunPos, bulletFrontPos))
            //{
            //    Debug.Log("Putting bullet in the preordained position (no hit)");
            //    Debug.Break();

            //    //transform.position = originalGunPos + (transform.up * FindObjectOfType<Gun>().bulletRange) * 0.5f;
            //    transform.position = originalGunPos + transform.up * (Vector3.Distance(originalGunPos, bulletFrontPos) * 0.5f);

            //    transform.localScale = new Vector3(
            //        transform.localScale.x,
            //        Vector3.Distance(originalGunPos, bulletFrontPos),
            //        transform.localScale.z
            //        );
            //}
        }
    }


    private void GetNewLerpTime()
    {
        if (speed <= 1f)
        {
            //Debug.Log("Changed speed to 1");
            //Debug.Break();
            lerpTime = 1;
        }

        else
        {
            lerpTime += (1 / speed) * Time.deltaTime;

            if (lerpTime >= 0.9)
            {
                //Debug.Log("Lerp time was greater than 0.9. Delta time: " +Time.deltaTime);
                //Debug.Log(Time.deltaTime);
                //Debug.Break();
            }
        }

        lerpTime = Mathf.Clamp01(lerpTime);
    }


    private void WaitToBeDeleted()
    {
        // If this bullet has already hit something, track how long it has been on screen for
        timeSinceHit += Time.deltaTime;

        // If this bullet has existed long enough to be deleted then delete it.
        if (timeSinceHit >= deleteTime)
        {
            // Move this bullet back to its holding location & make it tiny.
            transform.position = new Vector3(0, -500, 0);
            transform.localScale = new Vector3(1, 1, 1);

            readyToBeDeleted = false;
            hitSomething = false;
            lerpTime = 0f;
            isActive = false;
        }
    }


    private void MoveForward()
    {
        if (!isActive) return;

        Vector3 newFrontPosition = bulletFrontPos;
        Vector3 newBackPosition = bulletBackPos;

        SetTransformByEndPoints(newBackPosition, newFrontPosition);
    }


    /// <summary>
    /// Moves the bullet to the given position, rotation and scale, then sets it as active.
    /// </summary>
    /// <param name="_direction">The direction of the fired bullet.</param>
    /// <param name="_speed">The speed of the fired bullet.</param>
    /// <param name="_length">The length of the fired bullet.</param> 
    /// <param name = "_width">The width of the fired bullet.</param>
    public void GetFired(Vector3 _direction, Quaternion _rotation, float _speed, float _length, float _width)
    {
        // Reset this bullet's timer.
        timeSinceHit = 0f;
        hitSomething = false;
        readyToBeDeleted = false;
        lerpTime = 0f;

        //originalGunPos = gun.bulletSpawnTransform.position;
        //endPoint = originalGunPos + (_direction.normalized * gun.bulletRange);

        // Set scale and rotation.
        //maxLength = _length;
        transform.rotation = _rotation;

        speed = _speed;

        GetNewLerpTime();
        //Debug.Log(lerpTime);
        //if (lerpTime > 0.9f) Debug.Break();
        nextFrontPosition = Vector3.Lerp(originalGunPos, endPoint, lerpTime);
        maxLength = Vector3.Distance(originalGunPos, nextFrontPosition);
        transform.localScale = new Vector3(_width, maxLength * 0.5f, _width);
        nextBackPosition = nextFrontPosition + (-transform.up * (maxLength));

        // See if this next position would result in a hit.
        TestForHit(originalGunPos, nextFrontPosition);

        // If I've reached my maximum range, just pretend I hit something.
        if (lerpTime >= 1)
        {
            hitSomething = true;
        }

        if (hitSomething)
        {
            //Debug.Log("Hit something during first frame.");
            //Debug.Break();
            timeSinceHit = 0f;
            readyToBeDeleted = true;
        }

        // Go to the correct position.
        SetTransformByEndPoints(nextBackPosition, nextFrontPosition);

        // Set this bullet to active.
        isActive = true;
    }


    private void SetTransformByEndPoints(Vector3 back, Vector3 front)
    {
        // Make sure the length of the bullet is not greater than the distance that it's travelled.
        if (Vector3.Distance(back, front) > Vector3.Distance(originalGunPos, front))
        {
            //Debug.Log("Putting bullet in the preordained position (hit)");
            //Debug.Break();

            back = originalGunPos;
        }

        transform.position = Vector3.Lerp(back, front, 0.5f);

        transform.localScale = new Vector3(
            transform.localScale.x,
            Vector3.Distance(back, front) * 0.5f,
            transform.localScale.z
            );

        //Debug.DrawRay(transform.position, Vector3.up * 500f, Color.green);
        //Debug.Break();
    }


    //public void GetFired(Vector3 _position, Quaternion _rotation, Vector3 _scale)
    //{
    //    // Get proper transform values.
    //    transform.position = _position;
    //    transform.rotation = _rotation;
    //    transform.localScale = _scale;

    //    // Set this bullet to active.
    //    isOnScreen = true;

    //    // Reset this bullet's timer.
    //    timeOnScreen = 0f;
    //}


    //private void OnTriggerStay(Collider other)
    //{
    //    if (isOnScreen && !hitSomething)
    //    {
    //        // If the bullet hit an enemy...
    //        if (other.tag == "Enemy")
    //        {
    //            // Tell the enemy it was hurt.
    //            other.GetComponent<Enemy>().HP -= Random.Range(FindObjectOfType<Gun>().bulletDamageMin, FindObjectOfType<Gun>().bulletDamageMax);

    //            // Tell the score controller that the player hit an enemy with a bullet.
    //            FindObjectOfType<GameManager>().BulletHit();

    //            Instantiate(strikeParticles, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
    //            hitSomething = true;
    //        }

    //        else if (other.tag == "Obstacle")
    //        {
    //            Instantiate(strikeParticles, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
    //            hitSomething = true;
    //        }
    //    }
    //}
}
