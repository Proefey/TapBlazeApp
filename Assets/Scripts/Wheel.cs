using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WheelState{
    Inactive,
    Begin,
    Active,
    Calculate,
    Slow,
    End,
    Result
};
public class Wheel : MonoBehaviour
{
    /*Setting up the wheel*/
    //Number of sections in the wheel
    public int section_num;
    //Chance for each section of the wheel. In example, Life 30 Min is section 0 and sections go clockwise
    //If section_chances is length 0, assume equal chances for all sections
    public float[] section_chances;
    //Section objects
    public GameObject[] section_objs;
    //Wheel objects
    public GameObject[] wheel_objs;
    //Glowing Gif (FROM GIFER)
    public GameObject glow_sprite;
    //Angle difference between each section
    private float angle_diff;
    private Rigidbody2D rb;

    /*For spinning the wheel*/
    //Maximum speed the wheel rotates
    public float MaxAngularVelocity;
    //How fast the wheel rotates
    public float AngularAcceleration;
    //How long the wheel is guaranteed to spin
    public float SpinTime;
    //Variance in how long the wheel will spin
    public float SpinTimeVariance;
    private float timer;
    private float target_angle;
    private int winning_sector = 0;
    private WheelState WS = WheelState.Inactive;

    /*Debugging Functionality*/
    //Enable Debugging
    public bool debug_flag;
    //Force sector to win
    public int force_win_sector;

    // Start is called before the first frame update
    public void Start() {
        rb = GetComponent<Rigidbody2D>();
        //Error checking
        if (rb == null) {
            Debug.LogError("No Rigidbody found on " + gameObject.name);
            return;
        }
        if (section_num < 1){
            Debug.LogError("Invalid Number of Sections: " + section_num);
            return;
        }
        if (section_chances.Length != 0 && section_chances.Length != section_num){
            Debug.LogError("Incorrect length of section_chances. Expected: " + section_num + ". Array Length: " + section_chances.Length);
            return;
        }
        float chance_sum = 0;
        foreach(float i in section_chances){
            chance_sum += i;
        }
        if (section_chances.Length != 0 && Mathf.Abs(chance_sum - 100f) > 0.1){
            Debug.LogError("Chances do not add up to 100. Sum: " + chance_sum);
            return;
        }
        angle_diff = 360 / section_num;
    }

    // State machine
    void Update(){
        switch(WS){
            case WheelState.Inactive:
                break;
            case WheelState.Begin:
                WheelBegin();
                break;
            case WheelState.Active:
                WheelActive();
                break;
            case WheelState.Calculate:
                WheelCalculate();
                break;
            case WheelState.Slow:
                WheelSlow();
                break;
            case WheelState.End:
                WheelEnd();
                break;
            case WheelState.Result:
                WheelResult();
                break;
            default:
                break;
        }
        return;
    }


    //Begin wheel spinning
    public void StartWheel(){
        WS = WheelState.Begin;
        return;
    }

    private void WheelBegin(){
        //Goes counter-clockwise for a bounce effect
        rb.angularVelocity = AngularAcceleration / 5;
        WS = WheelState.Active;
        //Create timer for random amount of time to spin
        timer = SpinTime + Random.Range(SpinTimeVariance * -1f, SpinTimeVariance);
        return;
    }

    private void WheelActive(){
        //Accelerate
        if(Mathf.Abs(rb.angularVelocity) < MaxAngularVelocity){
            rb.angularVelocity -=  AngularAcceleration * Time.deltaTime;
        }
        
        //Timer to exit spinning
        if(timer < 0){
            WS = WheelState.Calculate;
        }
        else{
            timer -= Time.deltaTime;
        }
        return;
    }

    private void WheelCalculate(){
        winning_sector = win();
        //Target angle set slightly to the left of the center of the winning sector for the bouncing effect at the end
        target_angle = winning_sector * angle_diff + angle_diff / 3;
        WS = WheelState.Slow;
        return;
    }
    private void WheelSlow(){
        if(target_angle > 180){
            target_angle -= 360;
        }
        //Calculate Wheel rotation. Wheel rotation guaranteed to be negative
        float wheel_rotation = rb.rotation % 360;
        if(wheel_rotation > 180){
            wheel_rotation -= 180;
        }
        else if (wheel_rotation < -180){
            wheel_rotation += 360;
        }
        float wheeltargetdiff = Mathf.DeltaAngle(wheel_rotation, target_angle);

        //Slow down rotation a bit
        if(Mathf.Abs(rb.angularVelocity) > 100f){
            rb.angularVelocity *= 0.998f;
        }
        //Continue slowing down rotation when close to target angle
        else if(Mathf.Abs(wheeltargetdiff) < (angle_diff * 1.5)){
            rb.angularVelocity *= 0.998f;
            //If at target angle, go to end state
            if(Mathf.Abs(wheeltargetdiff) < 1f){
                WS = WheelState.End;
            }
        }
        return;
    }

    private void WheelEnd(){
        //Target angle set to center of winning sector
        target_angle = winning_sector * angle_diff + angle_diff / 2;
        if(target_angle > 180){
            target_angle -= 360;
        }

        //Recalculate wheel rotation
        float wheel_rotation = rb.rotation % 360;
        if(wheel_rotation > 180){
            wheel_rotation -= 180;
        }
        else if (wheel_rotation < -180){
            wheel_rotation += 360;
        }
        float wheeltargetdiff = Mathf.DeltaAngle(wheel_rotation, target_angle);
        //Debug.Log(winning_sector + " " + target_angle + " " + wheel_rotation);
        //Rotate wheel to Target Angle
        if(Mathf.Abs(wheeltargetdiff) > 1f){
            rb.angularVelocity = 20;
        }
        else{
            rb.angularVelocity = 0;
            rb.rotation = target_angle;
            //Disable all other objects
            for (int i = 0; i < section_num; i++){
                if(i != winning_sector){
                    section_objs[i].SetActive(false);
                }
            }
            foreach (GameObject i in wheel_objs){
                i.SetActive(false);
            }
            WS = WheelState.Result;
        }
        return;
    }

    private void WheelResult(){
        //Move winning sector image to middle of screen
        Vector3 center = new Vector3(0, 0, 0);
        section_objs[winning_sector].transform.position = Vector3.MoveTowards(section_objs[winning_sector].transform.position, center, 0.005f);
        if(section_objs[winning_sector].transform.localScale.x < 3f || section_objs[winning_sector].transform.localScale.y < 3f){
            Vector3 scale_change = new Vector3(0.0075f, 0.0075f, 0.0075f);
            section_objs[winning_sector].transform.localScale += scale_change;
        }
        //Enable glowing sprite
        glow_sprite.SetActive(true);
        return;
    }

    //Return which section the user won based on (optional) weighted chances
    public int win(){
        //Debug
        if(debug_flag){
            return force_win_sector - 1;
        }
        //Random number between 0 and 100
        float chance = Random.Range(0f, 100f);
        float threshold = 0;
        int winning_section = 0;

        //Equal Chances Given No section_chances array
        if (section_chances.Length == 0){
            float equal_chance = 100f / section_num;
            threshold = equal_chance;
            while(threshold < chance){
                if(winning_section == section_num - 1) break;
                winning_section += 1;
                threshold += equal_chance;
            }
        }
        //Weighted Chances
        else{
            threshold = section_chances[0];
            while(threshold < chance){
                if(winning_section == section_num - 1) break;
                winning_section += 1;
                threshold += section_chances[winning_section];
            }
        }
        // Return section that won
        return winning_section;
    }
}
