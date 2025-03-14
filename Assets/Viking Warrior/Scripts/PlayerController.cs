using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    [Header("Character Attributes:")]
    public float CHARACTER_MOVE_SPEED = 1.0f;
    public int CHARACTER_MAX_HEALTH = 100;
    int currentPlayerHealth;

    [Space]
    [Header("Character Stats:")]
    public Vector2 movementDirection;
    public float movementSpeed;

    [Space]
    [Header("References:")]
    public Rigidbody2D rb;
    public Animator animator;



    // Start is called before the first frame update
    void Start(){
        currentPlayerHealth = CHARACTER_MAX_HEALTH;

    }

    // Update is called once per frame
    void Update()
    {
        ProcessInputs();
        Move();
        Animate();


        // Flips the player if headed in other direction
        if (movementDirection.x < 0){
            Debug.Log("Facing Left");
            transform.localScale = new Vector3(-1,1,1);
        } else if (movementDirection.x>0){
            Debug.Log("Facing Right");
            transform.localScale = new Vector3(1,1,1);
        }


    }

    void ProcessInputs (){
        // Stores the direction (x,y) into a variable from user inputs 
        movementDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Caps the movement speed to the same in all directions, otherwise travelling diagonally would be faster than non-diagonally
        movementSpeed = Mathf.Clamp(movementDirection.magnitude, 0.0f, 1.0f);
        movementDirection.Normalize();

        if (Input.GetButtonDown("Fire1")) {
            if (movementSpeed == 0) {
                Attack("Slash");
            } else {
                Attack("Thrust");
            }
        } 

        if (Input.GetKeyDown("k")){
            DiffHealth(-20);
        }

 
    }

    void Move(){
        rb.velocity = movementDirection * movementSpeed * CHARACTER_MOVE_SPEED;
    }   

    void Animate(){
        if (movementDirection != Vector2.zero) {
            animator.SetFloat("Horizontal", movementDirection.x);
            animator.SetFloat("Vertical", movementDirection.y);
        }
        animator.SetFloat("Speed", movementSpeed);
    }
    
    void Attack(string attackType){
        Debug.Log(attackType);
        if (attackType == "Slash"){
            print ("Attack: Slash");
            animator.SetTrigger("Slash");
        }
        else if (attackType =="Thrust"){
                print ("Attack: Thrust");
                animator.SetTrigger("Thrust");
        }
    
    
    }

    void DiffHealth(int diffHealth){
        currentPlayerHealth = Mathf.Clamp(currentPlayerHealth + diffHealth, 0, CHARACTER_MAX_HEALTH);
        print ("current health:"+ currentPlayerHealth);

        if (currentPlayerHealth <= 0) {
            animator.SetTrigger("Death");
            print ("I'm dying!!");

            currentPlayerHealth = CHARACTER_MAX_HEALTH;
            print ("You've been revived with full health!");
        }
        else if (diffHealth < 0){
            animator.SetTrigger("Hit");
            print ("I'm hit!");
        }

        
    }
}
