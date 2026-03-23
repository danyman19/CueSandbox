using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    public GameObject[] slots;
    public Card[] openSlots = new Card[5];

    public GameObject[] playSlots;
    public Card[] openPlaySlots = new Card[3];

    public Queue<Card> deck = new Queue<Card>();
    public List<Card> fullDeck = new List<Card>();

    public GameObject cardPrefab;

    public Sprite[] cards;
    public string[] ids;

    public TMP_Text phaseText;
    
    private string[] phaseToStr = {"draw", "start", "energy", "play", "buff", "score", "return"};

    public TMP_Text roundText;
    public int round = 1;
    public int turn = 1;

    public bool[] roundResults;

    private int curRoundScore = 0;
    private int curTurnScore = 0;

    public TMP_Text turnScoreText;
    public TMP_Text[] arrRoundTxt;

    public int curPhase = 0;
    //draw: 0, start = 1, energy = 2, play = 3 | buff = 4, score = 5, return = 6

    public float timer = -1;
    public float val;

    public GameObject[] handPowerSquares;
    public GameObject[] playPowerSquares;

    public TMP_Text[] handSlotTxt;
    public TMP_Text[] playSlotTxt;

    public TMP_Text buffText;

    public static void Shuffle<T>(T[] array)
    {
        System.Random rng = new System.Random();
        int n = array.Length;
        while (n > 1) 
        {
            n--;
            int k = rng.Next(n + 1); // Pick a random index from 0 to n
            // Swap values
            T value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }

    void Setup() {
        //Generate Deck

        //ORDERING FOR high score lol/ocean mammals
        // int[] order = {11, 9, 4, 5, 7, 10, 2, 8, 17, 12, 16, 13, 14, 0, 18, 15, 3, 6};

        //ORDERING FOR paleo test
        // int[] order = {27, 25, 26, 28, 24, 21, 29, 20, 19, 22, 23};

        //ORDERING FOR great dying deck
        //carto jurassic cambrian camel squirrel
        //GD buffalo megalo, svalbard, arabain, thompsons
        //dolly beaver bat
        //morelet atomic tepe when pigs fly

        //             T1                  T2          T3          T1
        int[] order = {43, 32, 33, 35, 42, 45, 36, 44, 41, 34, 39, 38, 37, 40, 17, 46, 31, 30};
        Shuffle<int>(order);

        for(int j = 0; j < order.Length;j++)
        {
            int i = order[j];
            Card cur = new Card(fullDeck, Instantiate(cardPrefab), cards[i], ids[i], new Data(ids[i]));

            deck.Enqueue(cur);
            fullDeck.Add(cur);
        }
    }

    void Log()
    {
        string outPlay = "in play: ";
        string outOther = "in hand: ";

        for(int i = 0; i < 3; i++)
        {
            outPlay+= (openPlaySlots[i]==null?"null":openPlaySlots[i].GetID())+", ";
        }
        for(int i = 0; i < 5; i++)
        {
            outOther+= (openSlots[i]==null?"null":openSlots[i].GetID())+", ";
        }
        Debug.Log(outPlay);
        Debug.Log(outOther);
    }

    public void ClickCard(string id)
    {
        if(curPhase!=3) return;

        if (id == "END_TURN_BUTTON")
        {
            curPhase++;

            curRoundScore += curTurnScore;
            arrRoundTxt[round-1].text = "Round "+round+": "+curRoundScore;

            return;
        }

        Card cur = null;
        int pos = -1;

        for(int i = 0; i < 5; i++)
        {
            if(openSlots[i]!=null && openSlots[i].GetID()==id){
                cur = openSlots[i];
                pos = i;
            }
        }
        for(int i = 0; i < 3; i++)
        {
            if(openPlaySlots[i]!=null && openPlaySlots[i].GetID()==id){
                cur = openPlaySlots[i];
                pos = i;
            }
        }

        if (cur == null)
        {
            Debug.Log("no card was clicked??!! ERROR");
        }

        buffText.text = cur.buffText();

        if (cur.GetSlot() == 1)
        {
            //want to swap it in

            for(int i = 0; i < 3; i++)
            {
                if (openPlaySlots[i] == null)
                {
                    cur.SetSlot(2);
                    cur.GoTo(playSlots[i]);
                    cur.TriggerAbility("play", true);
                    cur.played = true;
                    
                    openSlots[pos] = null;
                    openPlaySlots[i] = cur;

                    ResetPower();

                    return;
                }
            }
        }
        else
        {
            //want to swap it out

            cur.SetSlot(1);
            cur.GoTo(slots[cur.pSlot]);
            cur.TriggerAbility("play", false);
            cur.played = false;

            openPlaySlots[pos] = null;
            openSlots[cur.pSlot] = cur;

            ResetPower();

            return;
        }
    }

    void ResetPower()
    {
        for(int i = 0; i < 5; i++)
        {
            if (openSlots[i] == null)
            {
                handPowerSquares[i].SetActive(false);
                handSlotTxt[i].text = "";
            }
            else
            {
                handPowerSquares[i].SetActive(true);
                handSlotTxt[i].text = ""+openSlots[i].GetPower();
            }
        }

        for(int i = 0; i < 3; i++)
        {
            if (openPlaySlots[i] == null)
            {
                playPowerSquares[i].SetActive(false);
                playSlotTxt[i].text = "";
            }
            else
            {
                playPowerSquares[i].SetActive(true);
                playSlotTxt[i].text = ""+openPlaySlots[i].GetPower();
            }
        }
        
        int sum = 0;
        for(int i = 0; i < 3; i++)
        {
            sum += openPlaySlots[i]==null?0:openPlaySlots[i].GetPower();
        }
        curTurnScore = sum;
        turnScoreText.text = "Current turn score: "+sum;
    }

    bool DrawCard()
    {
        // Card cur = deck.Dequeue();
        for(int i = 0; i < slots.Length; i++)
        {
            if (openSlots[i]==null)
            {
                Card cur = deck.Dequeue();
                cur.SetSlot(1);
                cur.GoTo(slots[i]);
                cur.SetActive(true);

                if(cur.data.name=="Confractosuchus") Debug.Log("setting confac to inhand true");
                cur.inHand = true;
                cur.played = false;
                cur.drawTriggered = false;
                // cur.TriggerAbility("draw", true);

                cur.pSlot = i;

                handPowerSquares[i].SetActive(true);

                openSlots[i] = cur;

                ResetPower();

                return true;
            }
        }

        return false;
    }
    
    int SlotsOpen()
    {
        int o = 0;
        for(int i = 0;i<5;i++) if(openSlots[i]==null) o++;
        return o;
    }

    public void incPhase()
    {
        curPhase++;
        timer = -1;
        
        if(curPhase==7){
            curPhase = 0;

            turn++;
            if (turn >= 4)
            {
                curRoundScore = 0;

                foreach(Card cur in fullDeck)
                {
                    cur.data.UpdateRoundBuffs(round);
                }
                
                round++;
                turn = 1;


                //Hard coded mammals buff
                if (round == 5)
                {
                    foreach(Card cur in fullDeck)
                    {
                        if(cur.c3("LMA")) cur.AddBuff(new Buff(1, -1, 100, "Arena Boost"), true);
                    }
                }
            }
        }
    }

    //draw: 0, start = 1, energy = 2, play = 3 | buff = 4, score = 5, return = 6
    public void DoPhases()
    {
        phaseText.text = "Current phase: "+phaseToStr[curPhase];

        if(timer==-1) timer = 0;
        else
        {
            timer += Time.deltaTime;

            if(timer > val)
            {
                timer -= val;

                if (curPhase == 0) {
                    if(SlotsOpen()==0){
                        foreach(Card c in openSlots)
                        {
                            if(!c.drawTriggered){
                                c.TriggerAbility("draw", true);
                                c.drawTriggered = true;
                            }
                        }

                        ResetPower();

                        incPhase();
                        return;
                    }else DrawCard();
                }
                if(curPhase==1 || curPhase==2 || curPhase==4 || curPhase==5) {
                    incPhase();
                    return;
                }

                if (curPhase == 6)
                {
                    List<Card> played = new List<Card>();
                    for(int i = 0; i < 3; i++)
                    {
                        if (openPlaySlots[i] != null)
                        {
                            Card cur = openPlaySlots[i];

                            cur.SetActive(false);
                            cur.EndTurn(turn, true);
                            played.Add(cur);
                            
                            cur.inHand = false;
                            cur.data.numPlayed++;
                            cur.data.UpdatePlayBuffs();

                            //bad practice, TODO: make a func for this
                            if (cur.data.album == "L" || cur.data.album=="P")
                            {
                                foreach(Card c in fullDeck)
                                {
                                    if (c.GetID() == "LMA064" && cur.data.album=="L")
                                    {
                                        c.data.abilites[0].b.power += 3;
                                    }
                                    if(c.GetID()=="PLB003" && cur.data.album == "P")
                                    {
                                        c.data.abilites[0].b.power -= 10;
                                        c.data.abilites[1].b.power += 6;
                                    }
                                }
                            }

                            deck.Enqueue(cur);

                            openPlaySlots[i] = null;

                            playPowerSquares[i].SetActive(false);

                            ResetPower();
                        }
                    }

                    foreach(Card cur in fullDeck)
                    {
                        if(played.Contains(cur)) continue;

                        cur.EndTurn(turn, false);
                    }

                    foreach(Card cur in played)
                    {
                        cur.TriggerAbility("return", true);
                    }

                    // foreach(Card cur in openSlots)
                    // {
                    //     if (cur != null)
                    //     {
                    //         cur.EndTurn(turn, false);
                    //         ResetPower();
                    //     }
                    // }

                    incPhase();
                    return;
                }
            }
        }
    }

    void Start()
    {
        // GameObject inst = Instantiate(card);
        // inst.GetComponent<SpriteRenderer>().sprite = sptest;

        Setup();
    }

    void Update()
    {
        // Vector2 mousePos = Mouse.current.position.ReadValue();
        // Debug.Log(mousePos);
        
        DoPhases();

        string txt = "GAME OVER";
        if(round<=5) txt = "Round "+round+", Turn: "+turn;
        roundText.text = txt;
    }
}

public class Card {
    private GameObject card;
    private List<Card> deck;
    private string id;
    private int slot;
    public int pSlot;

    public bool inHand, played;
    public bool drawTriggered;

    public List<Buff> buffs = new List<Buff>();
    
    public Data data;

    public Card(List<Card> deck, GameObject obj, Sprite img, string id, Data data)
    {
        this.deck = deck;
        this.card = obj;
        this.card.GetComponent<SpriteRenderer>().sprite = img;
        this.card.GetComponent<ClickScript>().id = id;
        this.id = id;
        this.slot = 0;
        this.inHand = false;
        this.played = false;

        this.data = data;

        this.card.SetActive(false);
    }

    public bool c3(string str)
    {
        return this.id.Substring(0,3)==str;
    }

    public void AddBuff(Buff b, bool pos)
    {
        for(int i = 0;i<buffs.Count;i++)
        {
            Buff curB = buffs[i];
            if(curB.turnsLeft==b.turnsLeft && curB.type == b.type&&curB.origin==b.origin)
            {
                if(pos) buffs[i].power += b.power;
                else buffs[i].power -= b.power;
                
                if(buffs[i].power == 0) buffs.RemoveAt(i);

                return;
            }
        }

        if(!pos) Debug.Log("ERROR, tried to remove buff but COULDNT FIND AGHGH");
        else{
            buffs.Add(b.copy());
        }
    }

    public void EndTurn(int turn, bool played)
    {
        for(int i = buffs.Count - 1; i >= 0; i--)
        {
            //0 = this turn, 1 = this round, 2 = x turns left, 3 = until played, 4 = perma
            int type = buffs[i].type;

            if(type==2) buffs[i].turnsLeft--;
            if(type==0 || type==1 && turn==3 || type==2 && buffs[i].turnsLeft==0 || type==3 && played) buffs.RemoveAt(i);
        }
    }

    public void TriggerAbility(string trigger, bool pos)
    {
        foreach(Ability ability in data.abilites)
        {
            if (ability.trigger == trigger)
            {
                foreach(Card c in deck)
                {
                    // if(this.data.name=="Jurassic Coast") Debug.Log(c.data.name+" "+c.inHand +" "+ (c.data.album=="P"));
                    if (ability.target(c))
                    {
                        // if(this.data.name=="Jurassic Coast") Debug.Log("applying to "+c.data.name);
                        c.AddBuff(ability.b, pos);
                    }
                }
            }
        }
    }

    public int GetPower()
    {
        int b = this.data.basePower;
        foreach(Buff buff in buffs){
            b+= buff.power;
        }
        if(b<0) b = 0;
        return b;
    }

    public string GetID()
    {
        return this.id;
    }

    public int GetSlot()
    {
        return slot;
    }

    public void SetSlot(int slot)
    {
        this.slot = slot;
    }

    public void SetActive(bool active)
    {
        this.card.SetActive(active);
    }

    public void GoTo(GameObject obj)
    {
        this.card.transform.position = obj.transform.position;
    }

    public string buffText()
    {
        string o = "["+id+"] => "+this.data.name+" ("+this.data.album;
        foreach(Buff b in buffs)
        {
            o+="\n";
            o+=b.origin+": "+b.power+" "+typeToStr(b);
        }
        return o;
    }
    public string typeToStr(Buff b)
    {
        int type = b.type;
        if(type==0) return "this turn";
        if(type==1) return "this round";
        if(type==2) return "- "+b.turnsLeft+" turns left";
        if(type==3) return "until played";
        if(type==4) return "permanent";
        return "unkown type";
    }

    //gm = FindObjectOfType<GameManager>();
}

public class Ability
{
    public string trigger;
    public Func<Card, bool> req, target;
    public Buff b;

    public Ability(string trigger, Func<Card, bool> target, Buff b)
    {
        this.trigger = trigger;
        // this.req = req;
        this.target = target;
        this.b = b;
    }
}

public class Buff
{
    //0 = this turn, 1 = this round, 2 = x turns left, 3 = until played, 4 = perma
    public int type;
    public int turnsLeft;
    public int power;

    public string origin;

    public Buff(int t, int tl, int p, string o)
    {
        this.type = t;
        this.turnsLeft = tl;
        this.power = p;
        this.origin = o;
    }

    public Buff copy()
    {
        return new Buff(this.type, this.turnsLeft, this.power, this.origin);
    }
}

public class Data
{
    public string id;
    public int energy, basePower;
    public string album, collection, rarity, rType, name;

    public int numPlayed;

    // private Card c;

    public List<Ability> abilites = new List<Ability>();

    public Data(string id)
    {
        this.id = id;
        this.numPlayed = 0;

        SetAttributes();
    }

    //for dynamic buffs ()
    public void UpdatePlayBuffs()
    {
        if (id == "OMA043")
        {
            abilites[0].b.power+=13;
        }
        if(id == "LMA055")
        {
            abilites[0].b.power = 10;
        }
        if(id == "LMA061")
        {
            abilites[0].b.power = 16;
        }
        // if(id == "PAN054")
        // {
        //     //HARD CODED
        //     abilites[0].b.power = 12;
        // }
        // if(id == "LMA060")
        // {
        //     if(this.numPlayed==1) abilites[0].b.power = 0;
        //     else abilites[0].b.power = 35;
        // }
    }

    public void UpdateRoundBuffs(int round)
    {
        //HARD CODED
        if(id == "OMA044")
        {
            abilites[0].b.power+=30;
            if(round==1 || round == 2)
            {
                abilites[1].b.power+=20;
            }
        }
    }

    public void SetAttributes()
    {
        //0 = this turn, 1 = this round, 2 = x turns left, 3 = until played, 4 = perma
        SetAlbCol();

        //GREEN BLUE OCEAN MAMMALS
        if (id == "ECF077"){
            SetInfo("Zhou Dunyi",7, 60, "leg", "lim");
            AddAbility("play",(x => x.data.album!="E"), new Buff(3,-1,17, this.name));
        }
        if (id == "LAL045"){
            SetInfo("Green Ringtail Possum", 8, 76, "rare", "lim");
            AddAbility("return", (x => x.inHand&&!x.played), new Buff(4,-1,17, this.name));
        }
        if (id == "LBI016"){
            SetInfo("Bluejay", 7, 20, "leg", "base");
            AddAbility("play", (x => x.data.album=="O"), new Buff(3,-1,10, this.name));
        }
        if (id == "LBI088"){
            SetInfo("Eurasian Blue Tit", 4, 38, "rare", "lim");
            //HARD CODED RANDOM ABILITY
            AddAbility("play", (x => x.GetID()=="ECF077"||x.c3("OMA")), new Buff(3,-1,14, this.name));
        }
        if (id == "LMC037"){
            SetInfo("Jinn", 7, 57, "leg", "lim");
            AddAbility("play", (x => x.data.album=="O"), new Buff(3,-1,15, this.name));
        }
        if (id == "LPL001"){
            SetInfo("Petunia", 4, 42, "leg", "lvl");
            AddAbility("return", (x => x.c3("LPL")||x.c3("OMA")), new Buff(3,-1,16, this.name));
        }
        if (id == "LPR050"){
            SetInfo("Golden Lion Tamarin", 6, 67, "leg", "lim");
            AddAbility("draw", (x => x.data.album!="L"&&x.inHand), new Buff(3,-1,16, this.name));
            AddAbility("play", (x => x.data.album!="L"&&x.inHand), new Buff(3,-1,16, this.name));
            AddAbility("return", (x => x.data.album!="L"&&x.inHand&&!x.played), new Buff(3,-1,16, this.name));
        }
        if (id == "LTT049"){
            SetInfo("Lone Cyprus", 7, 64, "epic", "lim");
            //HARD CODED RANDOM ABILITY
            AddAbility("draw", (x => x.c3("OMA")), new Buff(3,-1,22, this.name));
        }
        if (id == "LVE029"){
            SetInfo("Greenbottle Blue Tarantula", 5, 49, "epic", "lim");
            AddAbility("play", (x => x.data.album=="L"||x.data.album=="O"), new Buff(2,3,18, this.name));
        }
        if (id == "MYSE005"){
            SetInfo("The Great Flood", 7, 50, "mythic", "base");
            AddAbility("play", (x => x.data.album=="O"), new Buff(2,3,48, this.name));
        }
        if (id == "ODE044"){
            SetInfo("Cauliflower Jelly", 6, 52, "leg", "lim");
            AddAbility("play", (x => x.data.album=="O"||x.data.album=="L"), new Buff(4,-1,10, this.name));
        }
        if (id == "OMA002"){
            SetInfo("Polar Bear", 7, 70, "epic", "base");
            AddAbility("play", (x => x.GetID()=="OMA002"), new Buff(0,-1,34, this.name));
        }
        if (id == "OMA039"){
            SetInfo("Dall's Porpoise", 3, 21, "leg", "base");
            AddAbility("draw", (x => x.c3("OMA")), new Buff(3,-1,24, this.name));
        }
        if (id == "OMA042"){
            SetInfo("Ringed Seal", 5, 55, "leg", "lim");
            //HARD CODED RANDOM ABILITY
            AddAbility("play", (x => x.c3("OMA")), new Buff(4,-1,25, this.name));
        }
        if (id == "OMA043"){
            SetInfo("African Manatee", 7, 62, "leg", "lim");
            AddAbility("play", (x => x.c3("OMA")), new Buff(3,-1,13, this.name));
        }
        if (id == "OMA044"){
            SetInfo("Hawaiian Monk Seal", 8, 60, "leg", "lim");
            //+30 for each round completed
            AddAbility("play", (x => x.GetID()=="OMA044"), new Buff(4,-1,0, this.name));
            //+20 for each round lost
            AddAbility("play", (x => x.c3("OMA")&&x.GetID()!="OMA044"&&x.inHand), new Buff(4,-1,0, this.name));
        }
        if (id == "OMA047"){
            SetInfo("Habor Porpoise", 3, 23, "leg", "lim");
            //HARD CODED
            AddAbility("play", (x => x.data.album=="O"||x.data.album=="L"), new Buff(3,-1,17, this.name));
        }
        if (id == "ORE034"){
            SetInfo("Morelet's Crocodile",7, 65, "epic", "lim");
            //HARD CODED
            AddAbility("draw", (x => x.data.album=="O"||x.data.album=="L"), new Buff(3,-1,19, this.name));
        }
        if(id == "LMC109")
        {
            SetInfo("Faun", 3, 29, "leg", "lvl");
            AddAbility("play", (x=>x.inHand), new Buff(4, -1, 17, this.name));
        }

        //PALEO TEST
        if(id == "FPA001")
        {
            SetInfo("Jurassic Coast", 5, 53, "fusion", "fusion");
            AddAbility("draw", (x=>x.inHand && x.data.album=="P"), new Buff(3, -1, 30, this.name));
        }
        if(id == "PAN015")
        {
            SetInfo("Woolly Mammoth", 8, 83, "leg", "lim");
            AddAbility("play", (x=>x.c3("PAN")&&x.data.basePower>=60), new Buff(3, -1, 24, this.name));
        }
        if(id == "PAN016")
        {
            SetInfo("Megacerops", 3, 27, "rare", "lim");
            AddAbility("play", (x=>x.c3("PAN")), new Buff(3, -1, 20, this.name));
        }
        if(id == "PAN032")
        {
            SetInfo("Dodo", 7, 32, "leg", "lim");
            AddAbility("draw", (x=>x.data.album=="P"), new Buff(0, -1, 33, this.name));
        }
        if(id == "PAN046")
        {
            SetInfo("Confractosuchus", 8, 70, "leg", "lim");
            AddAbility("draw", (x=>x.GetID()=="PAN046"), new Buff(3, -1, 20*5, this.name));
        }
        if(id == "PAN064")
        {
            SetInfo("Falkland Islands Wolf", 8, 140, "leg", "lim");
        }
        if(id == "PFF029")
        {
            SetInfo("Yanornis", 3, 30, "leg", "lim");
            AddAbility("play", (x=>x.data.album=="P"), new Buff(3, -1, 18, this.name));
        }
        if(id == "PHE050")
        {
            SetInfo("Torosaurus", 9, 80, "leg", "lim");
            AddAbility("play", (x=>x.data.album=="P"), new Buff(3, -1, 18, this.name));
        }
        if(id == "PLB012")
        {
            SetInfo("Flaming Cliffs", 6, 25, "leg", "lim");
            //hard coded (im lazy)
            AddAbility("draw", (x=>x.data.album=="P"), new Buff(4, -1, 10, this.name));
            AddAbility("play", (x=>x.data.album=="P"), new Buff(3, -1, 10, this.name));
        }
        if(id == "PMO031")
        {
            SetInfo("Tully Monster", 7, 69, "leg", "lim");
            AddAbility("draw", (x=>x.data.basePower>=70), new Buff(3, -1, 22, this.name));
        }
        if(id == "PMO046")
        {
            SetInfo("Ichthyostega", 7, 70, "leg", "lim");
            AddAbility("play", (x=>x.c3("PAN")), new Buff(3, -1, 25, this.name));
        }

        //great dying deck
        //0 = this turn, 1 = this round, 2 = x turns left, 3 = until played, 4 = perma

        if(id == "ACLG007")
        {
            SetInfo("When Pigs Fly", 8, 65, "leg", "lim");
            AddAbility("return", (x=>x.c3("LMA")), new Buff(2, 7, 19, this.name));
        }
        if(id == "EWW038")
        {
            SetInfo("Gobekli Tepe", 5, 47, "rare", "lim");
            AddAbility("return", (x=>x.c3("LMA")), new Buff(3, -1, 23, this.name));
        }
        if(id == "FPA003")
        {
            SetInfo("The Jurassic Period", 6, 36, "fusion", "fusion");
            AddAbility("return", (x=>x.data.album=="L"), new Buff(3, -1, 10, this.name));
        }
        if(id == "FPA008")
        {
            SetInfo("The Cambrian Explosion", 5, 36, "fusion", "fusion");
            AddAbility("draw", (x=>x.data.album=="P"||x.data.album=="L"||x.data.album=="O"), new Buff(3, -1, 5, this.name));
            AddAbility("return", (x=>x.data.album=="P"||x.data.album=="L"||x.data.album=="O"), new Buff(3, -1, 5, this.name));
        }
        if(id == "LMA037")
        {
            SetInfo("Arabian Horse", 9, 80, "leg", "lim");
            AddAbility("play", (x=>x.GetID()=="LMA037"), new Buff(1, -1, 100, this.name));
        }
        if(id == "LMA050")
        {
            SetInfo("Bactrian Camel", 8, 70, "leg", "base");
            //HARD CODED
            AddAbility("play", (x=>x.GetID()=="LMA050"), new Buff(0, -1, 55*2, this.name));
        }
        if(id == "LMA055")
        {
            SetInfo("Cape Buffalo", 7, 67, "epic", "lim");
            AddAbility("play", (x=>x.c3("LMA")), new Buff(4, -1, 0, this.name));
        }
        if(id == "LMA060")
        {
            SetInfo("Beaver", 5, 50, "leg", "lim");
            AddAbility("draw", (x=>x.inHand), new Buff(0, -1, 35, this.name));
        }
        if(id == "LMA061")
        {
            SetInfo("Dolly the Sheep", 6, 16, "leg", "lim");
            AddAbility("draw", (x=>x.inHand), new Buff(4, -1, 8, this.name));
        }
        if(id == "LMA063")
        {
            SetInfo("Thomson's Gazelle", 8, 60, "leg", "lim");
            AddAbility("play", (x=>x.c3("LMA")), new Buff(3, -1, 18, this.name));
        }
        if(id == "LMA064")
        {
            SetInfo("Greater Horseshoe Bat", 4, 28, "epic", "lim");
            AddAbility("draw", (x=>x.GetID()=="LMA064"), new Buff(3, -1, 0, this.name));
        }
        if(id == "LMA078")
        {
            SetInfo("Svalbard Reindeer", 7, 35, "leg", "lim");
            AddAbility("play", (x=>x.inHand&&x.data.album=="L"), new Buff(4, -1, 14, this.name));
        }
        if(id == "LMA082")
        {
            SetInfo("Eastern Grey Squirrel", 5, 36, "leg", "base");
            AddAbility("draw", (x=>x.GetID()=="LMA082"), new Buff(0, -1, 90, this.name));
            AddAbility("play", (x=>x.data.album=="L"), new Buff(3, -1, -6, this.name));
        }
        if(id == "PAN054")
        {
            //HARD CODED
            SetInfo("Cartorhynchus", 8, 68, "rare", "lim");
            AddAbility("play", (x=>x.data.album!="P"), new Buff(3, -1, 12, this.name));
        }
        if(id == "PIC010")
        {
            SetInfo("Megaloceros", 5, 56, "leg", "lim");
            AddAbility("draw", (x=>x.c3("LMA")), new Buff(3, -1, 24, this.name));
        }
        if(id == "PLB003")
        {
            SetInfo("Great Dying", 9, 78, "leg", "lim");
            // x-10
            AddAbility("return", (x=>x.data.album=="P"), new Buff(4, -1, 0, this.name));
            // x6
            AddAbility("return", (x=>x.data.album=="L"||x.data.album=="O"), new Buff(4, -1, 0, this.name));
        }
        if(id == "SDD042")
        {
            SetInfo("Atomic Theory", 7, 36, "leg", "lim");
            AddAbility("play", (x=>x.data.album=="L"), new Buff(3, -1, 20, this.name));
        }

        //beaver's second should NOT trigger
    }
    public void AddAbility(string trigger, Func<Card, bool> target, Buff b)
    {
        abilites.Add(new Ability(trigger, target, b));
    }

    public void SetInfo(string name, int e, int bp, string r, string rt)
    {
        this.name = name;
        this.energy = e;
        this.basePower = bp;
        this.rarity = r;
        this.rType = rt;
    }

    public void SetAlbCol()
    {
        //MYTHICS
        if (id.Substring(0, 2) == "MY")
        {
            if (id.Substring(2, 2) == "SE")
            {
                album = "O";
            }

            collection = id.Substring(0,4);

            return;
        }

        //FUSION
        if(id.Substring(0,1)== "F")
        {
            if(id.Substring(1, 2) == "PA")
            {
                album = "P";
            }

            collection = id.Substring(0,3);

            return;
        }

        //when pigs fly ugh
        if (id == "ACLG007")
        {
            album = "A";
            collection = "CLG";
            return;
        }

        album = id.Substring(0, 1);
        collection = id.Substring(1,2);
    }


}