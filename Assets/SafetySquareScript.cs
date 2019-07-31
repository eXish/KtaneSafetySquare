﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KModkit;

public class SafetySquareScript : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo Bomb;

    public KMSelectable water;
    public KMSelectable powder;
    public KMSelectable foam;
    public KMSelectable chem;
    public KMSelectable co2;
    public TextMesh redText;
    public TextMesh blueText;
    public TextMesh yellowText;
    public TextMesh whiteText;
    public TextMesh whitestrike;
    public GameObject barTop;
    public GameObject barBottom;
    public Material[] ledOptions;
    public Renderer led;
    public GameObject ledLight;
    public KMSelectable redButton;
    public KMSelectable blueButton;
    public KMSelectable yellowButton;
    public KMSelectable whiteButton;
    public KMHighlightable redHL;
    public KMHighlightable blueHL;
    public KMHighlightable yellowHL;
    public KMHighlightable whiteHL;
    public string[] buttonNames;

    //logging
    static int moduleIdCounter = 1;
    int moduleID;
    private bool moduleSolved;
    bool stageTwo;

    int redNum;
    int blueNum;
    int yellowNum;
    int answer;
    int stage;
    string fire;
    string ans1;
    string ans2;
    string ans3;
    string ans4;

    private string[] table1col1 = new string[] { "R", "W", "Y", "B", "Y" };
    private string[] table1col2 = new string[] { "W", "R", "W", "Y", "B" };
    private string[] table1col3 = new string[] { "Y", "B", "R", "R", "W" };
    private string[] table2red1 = new string[] { "R", "W", "Y", "B"};
    private string[] table2red2 = new string[] { "R", "W", "R", "W" };
    private string[] table2white1 = new string[] { "B", "R", "W", "Y" };
    private string[] table2white2 = new string[] { "Y", "W", "Y", "Y" };
    private string[] table2blue1 = new string[] { "Y", "W", "R", "B" };
    private string[] table2blue2 = new string[] { "B", "R", "Y", "R" };
    private string[] table2yellow1 = new string[] { "W", "Y", "R", "R" };
    private string[] table2yellow2 = new string[] { "B", "W", "Y", "B" };

    void Awake()
    {
        moduleID = moduleIdCounter++;

        water.OnInteract += delegate () { PressWater(); return false; };
        powder.OnInteract += delegate () { PressPowder(); return false; };
        foam.OnInteract += delegate () { PressFoam(); return false; };
        chem.OnInteract += delegate () { PressChem(); return false; };
        co2.OnInteract += delegate () { PressCo2(); return false; };
        redButton.OnInteract += delegate () { RedPress(); return false; };
        blueButton.OnInteract += delegate () { BluePress(); return false; };
        yellowButton.OnInteract += delegate () { YellowPress(); return false; };
        whiteButton.OnInteract += delegate () { WhitePress(); return false; };
    }

    // Use this for initialization
    void Start()
    {
        Disables();
        GetAns();
    }


    void GetAns()
    {

        //square generation
        int redNum = UnityEngine.Random.Range(0, 5);
        int blueNum = UnityEngine.Random.Range(0, 5);
        int yellowNum = UnityEngine.Random.Range(0, 5);
        int whiteNum = UnityEngine.Random.Range(0, 4);
        redText.text = redNum.ToString();
        blueText.text = blueNum.ToString();
        yellowText.text = yellowNum.ToString();
        whitestrike.text = " ";
        if (whiteNum == 0)
        { whiteText.text = " "; Debug.LogFormat("safteySquare #{0}: No special white rules", moduleID); }
        else if (whiteNum == 1)
        { whiteText.text = "W"; whitestrike.text = "_"; Debug.LogFormat("safteySquare #{0}: Special rule(W): don't use water", moduleID); }
        else if (whiteNum == 2)
        { whiteText.text = "OX"; Debug.LogFormat("safteySquare #{0}: Special rule(OX): don't use foam", moduleID); }
        else if (whiteNum == 3)
        { whiteText.text = "SA"; Debug.LogFormat("safteySquare #{0}: Special rule(SA): don't use co2", moduleID); }

        int numSum = redNum + blueNum + yellowNum;

                                        //CLACULATING FIRE TYPE
        if (numSum < 5)
        {
            Debug.LogFormat("safteySquare #{0}: Sum is less than 4, using table A", moduleID);
            //left table
            if (redNum == 0)
            {fire = "B"; }
            else if (yellowNum > redNum)
            { fire = "A"; }
            else if (whiteNum == 0)
            { fire = "K"; }
            else if (GetComponent<KMBombInfo>().IsIndicatorOn("FRK") || GetComponent<KMBombInfo>().IsIndicatorOn("IND"))
            { fire = "C"; }
            else { fire = "D"; }
        }
        else if (numSum == 5 || numSum == 6)
        {
            Debug.LogFormat("safteySquare #{0}: Sum is 5 or 6, Using table B", moduleID);
            //middle
            if (blueNum < 3)
            { fire = "A"; }
            else if (yellowNum == blueNum)
            { fire = "B"; }
            else if (redNum == GetComponent<KMBombInfo>().GetPortPlateCount())
            { fire = "C"; }
            else if (GetComponent<KMBombInfo>().IsIndicatorOn("CAR") || GetComponent<KMBombInfo>().IsIndicatorOn("BOB"))
            { fire = "D"; }
            else { fire = "K"; }
        }
        else
        {
            Debug.LogFormat("safteySquare #{0}: Sum is larger than 6, using table C", moduleID);
            //right
            if (GetComponent<KMBombInfo>().GetPortCount() > 4)
            { fire = "C"; }
            else if (numSum > 9)
            { fire = "D"; }
            else if (yellowNum == 4)
            { fire = "K"; }
            else if (GetComponent<KMBombInfo>().GetSerialNumberLetters().Any(x => x == 'A' || x == 'B' || x == 'C' || x == 'D' || x == 'K'))
            { fire = "A"; }
            else { fire = "B"; }
        }
                                                                 //CALCULATING CORRECT EXTINGIUSHER
        //NO VOWEL

        if (GetComponent<KMBombInfo>().GetSerialNumberLetters().All(x => x != 'A' && x != 'E' && x != 'I' && x != 'O' && x != 'U'))
        {
            Debug.LogFormat("safteySquare #{0}: No Vowel found", moduleID); //NO VOWEL
            if (fire == "A")
            {
                if (whiteNum != 1)
                { answer = 1; }
                else { answer = 3; }
            }
            else if (fire == "B")
            {
                if (whiteNum !=3) { answer = 3;}
                else if (whiteNum != 2) { answer = 5;}
                else { answer = 2;}
            }
            else if (fire == "C")
            {
                if (whiteNum !=2) { answer = 5;}
                else { answer = 2;}
            }
            else if (fire == "D")
            { answer = 2; }
            else { answer = 4;}
            
        }
        else
        {
            //VOWEL
            Debug.Log("VOWEL DETECTED");
            if (fire == "A") { answer = 4; }
            else if (fire == "B")
            {
                if (whiteNum != 3) { answer = 5; }
                else if (whiteNum != 2) { answer = 3; }
                else { answer = 2; }
            }
            else if (fire == "C")
            {
                if (whiteNum != 3) { answer = 5; }
                else { answer = 2; }
            }
            else if (fire == "D")
            { answer = 2; }
            else { answer = 4; }
        }
        //LOGGING
        Debug.LogFormat("safteySquare #{0}: Type {1} fire present", moduleID, fire);
        Debug.LogFormat("safteySquare #{0}: Stage one correct extinguisher is: button {1}, {2}", moduleID, answer, buttonNames[answer]);

    }
    //Button Presses
    void PressWater()
    {
        if (moduleSolved) { return; }
        water.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("safteySquare #{0}: You Pressed Water", moduleID);
        if (answer == 1 && stageTwo == false) { audio.PlaySoundAtTransform("fireSound", transform); Debug.LogFormat("safteySquare #{0}: That is correct", moduleID); StageTwo(); }
        else { WrongButton(); } 
    }
    void PressPowder()
    {
        if(moduleSolved) { return; }
        powder.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("safteySquare #{0}: You Pressed Powder", moduleID);
        if (answer == 2 && stageTwo == false) { audio.PlaySoundAtTransform("fireSound", transform); Debug.LogFormat("safteySquare #{0}: That is correct", moduleID); StageTwo(); }
        else { WrongButton(); }
    }
    void PressFoam()
    {
        if (moduleSolved) { return; }
        foam.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("safteySquare #{0}: You Pressed Foam", moduleID);
        if (answer == 3 && stageTwo == false) { audio.PlaySoundAtTransform("fireSound", transform); Debug.LogFormat("safteySquare #{0}: That is correct", moduleID); StageTwo(); }
        else { WrongButton(); }
    }
    void PressChem()
    {
        if (moduleSolved) { return; }
        chem.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("safteySquare #{0}: You Pressed Wet Chemical", moduleID);
        if (answer == 4 && stageTwo == false) { audio.PlaySoundAtTransform("fireSound", transform); Debug.LogFormat("safteySquare #{0}: That is correct", moduleID); StageTwo(); }
        else { WrongButton(); }
    }
    void PressCo2()
    {
        if (moduleSolved) { return; }
        co2.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("safteySquare #{0}: You Pressed Co2", moduleID);
        if (answer == 5 && stageTwo == false) { audio.PlaySoundAtTransform("fireSound", transform); Debug.LogFormat("safteySquare #{0}: That is correct", moduleID); StageTwo(); }
        else { WrongButton(); }
    }

    void Disables()  //DISABILING STAGE TWO STUFF FOR STAGE ONE
    {
        ledLight.SetActive(false);
        redHL.gameObject.SetActive(false);
        blueHL.gameObject.SetActive(false);
        yellowHL.gameObject.SetActive(false);
        whiteHL.gameObject.SetActive(false);
    }

    void StageTwo() // STAGE TWO ACTIVATED
    {
        stageTwo = true;
        Debug.LogFormat("safteySquare #{0}: Stage one passed", moduleID);
        barTop.SetActive(false);
        int ledCol = UnityEngine.Random.Range(1,4);//LED COLOR
        led.material = ledOptions[ledCol];
        ledLight.SetActive(true);
        yellowText.text = " "; //remove square text
        redText.text = " ";
        blueText.text = " ";
        whiteText.text = " ";
        whitestrike.text = " ";
        redHL.gameObject.SetActive(true);
        blueHL.gameObject.SetActive(true);
        yellowHL.gameObject.SetActive(true);
        whiteHL.gameObject.SetActive(true);
        stage = 1; //part two stage 1
        redButton.gameObject.transform.localPosition = new Vector3(-.0017f, 0.01f, 0.0317f);
        blueButton.gameObject.transform.localPosition = new Vector3(-.0017f, 0.01f, -0.0017f);
        yellowButton.gameObject.transform.localPosition = new Vector3(0.0317f, 0.01f, 0.0317f);
        whiteButton.gameObject.transform.localPosition = new Vector3(0.0317f, 0.01f, -0.0017f);
        answer = answer - 1;
        //calculate answer
        //ans1
        if (ledCol == 3) 
        { ans1 = table1col1[answer]; }
        else if (ledCol == 1) { ans1 = table1col2[answer]; }
        else { ans1 = table1col3[answer]; }
        //ans2
        if (GetComponent<KMBombInfo>().IsIndicatorOff("BOB") || GetComponent<KMBombInfo>().IsIndicatorOff("IND")) 
        { ans2 = "W"; }
        else if (GetComponent<KMBombInfo>().GetBatteryCount() > 3)
        { ans2 = "Y"; }
        else if (GetComponent<KMBombInfo>().GetPortCount(Port.DVI) > 0 || GetComponent<KMBombInfo>().GetPortCount(Port.RJ45) > 0)
        { ans2 = "R"; }
        else { ans2 = "B"; }
        //ans3+4
        int col;
        if (ans2 == "R") { col =  0; }
        else if (ans2 == "Y") { col = 1; }
        else if (ans2 == "W") { col = 2; }
        else  { col = 3; }
        if (ans1 == "R") { ans3 = table2red1[col]; ans4 = table2red2[col]; }
        else if (ans1 == "W") { ans3 = table2white1[col]; ans4 = table2white2[col]; }
        else if (ans1 == "B") { ans3 = table2blue1[col]; ans4 = table2blue2[col]; }
        else  { ans3 = table2yellow1[col]; ans4 = table2yellow2[col]; }
        //logging
        Debug.Log("answer 1 is: " + ans1);
        Debug.Log("answer 2 is: " + ans2);
        Debug.Log("answer 3 is: " + ans3);
        Debug.Log("answer 4 is: " + ans4);
        answer = answer++;
    }
    void RedPress()                 //STAGE TWO BUTTONS
    {
        if (moduleSolved) { return; }
        redButton.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("safteySquare #{0}: You Pressed Red", moduleID);
        if (ans1 == "R" && stage == 1) { stage = 2; }
        else if (ans2 == "R" && stage == 2) { stage = 3; }
        else if (ans3 == "R" && stage == 3) { stage = 4; }
        else if (ans4 == "R" && stage == 4) { Solve(); }
        else { WrongButton(); stage = 1; }
    }
    void BluePress()
    {
        if (moduleSolved) { return; }
        blueButton.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("safteySquare #{0}: You Pressed Blue", moduleID);
        if (ans1 == "B" && stage == 1) { stage = 2; }
        else if (ans2 == "B" && stage == 2) { stage = 3; }
        else if (ans3 == "B" && stage == 3) { stage = 4; }
        else if (ans4 == "B" && stage == 4) { Solve(); }
        else { WrongButton(); stage = 1; }
    }
    void YellowPress()
    {
        if (moduleSolved) { return; }
        yellowButton.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("safteySquare #{0}: You Pressed Yellow", moduleID);
        if (ans1 == "Y" && stage == 1) { stage = 2; }
        else if (ans2 == "Y" && stage == 2) { stage = 3; }
        else if (ans3 == "Y" && stage == 3) { stage = 4; }
        else if (ans4 == "Y" && stage == 4) { Solve(); }
        else { WrongButton(); stage = 1; }
    }
    void WhitePress()
    {
        if (moduleSolved) { return; }
        whiteButton.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("safteySquare #{0}: You Pressed White", moduleID);
        if (ans1 == "W" && stage == 1) { stage = 2; }
        else if (ans2 == "W" && stage == 2) { stage = 3; }
        else if (ans3 == "W" && stage == 3) { stage = 4; }
        else if (ans4 == "W" && stage == 4) { Solve(); }
        else { WrongButton(); stage = 1; }
    }

    void WrongButton()
    {
        GetComponent<KMBombModule>().HandleStrike();
        Debug.LogFormat("safteySquare #{0}: That is incorrect", moduleID);
    }

    void Solve()                    //MODULE SOLVED
    {
        moduleSolved = true;
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        GetComponent<KMBombModule>().HandlePass();
        barBottom.SetActive(false);
        ledLight.SetActive(false);
        led.material = ledOptions[0];
        Debug.LogFormat("safteySquare #{0}: Module Solved", moduleID);
    }
}