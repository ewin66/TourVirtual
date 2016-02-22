﻿using UnityEngine;
using System.Collections;



public class AchievementsAPI{ 
    public class Achievement{
        public string GUID;
        public string Name;
        public string IName;
        public string Description;
        public int Level;
        public string Image;
        public int Points;
        public bool earned;
        public Achievement(string _GUID, string _Name, string _IName, string _Description, int _Level, int _Points, string _Image ) {
            GUID = _GUID;
            Name = _Name;
            IName = _IName;
            Description = _Description;
            Level = _Level;
            Points = _Points;
            Image = _Image;
            earned = false;
        }
    }
    public Hashtable Achievements;

    public int TotalAchievements;
    public int EarnedAchievements;

    public IEnumerator AwaitRequest()
    {
        TotalAchievements = 0;
        Achievements = new Hashtable();
        yield return Authentication.AzureServices.AwaitRequestGet(string.Format("api/v1/achievements?achievementConfigurationType=VIRTUALTOUR&idClient={0}&language={1}", Authentication.IDClient, Authentication.AzureServices.MainLanguage), (res) =>
        {
            if (res != "null")
            {
                Debug.LogError(">>> Achievements " + res);
                ArrayList results = JSON.JsonDecode(res) as ArrayList;
                foreach (Hashtable ele in results)
                {
                    string guid = ele["IdAchievement"] as string;
                    string iname = ele["Name"] as string;
                    string name = ((ele["Description"] as ArrayList)[0] as Hashtable)["Description"] as string;
                    int points = (int)ele["Points"];
                    int level = (int)ele["Level"];
                    string description = ((ele["LevelName"] as ArrayList)[0] as Hashtable)["Description"] as string;
                    string imageUrl = ele["ImageUrl "] as string;
                    // string rule = ((ele["Rules"] as ArrayList)[0] as Hashtable)["IdAction"] as string;
                    Achievement tmp = new Achievement(guid, name, iname, description, level, points, imageUrl);
                    Achievements.Add(guid + "|" + level, tmp);
                    TotalAchievements++;
                }
            }
        });

        // Consulta mis logros.
        yield return Authentication.Instance.StartCoroutine(AwaitAchievementEarned(false));
    }

    public IEnumerator AwaitAchievementEarned(bool refresh =true) {
        bool needRequest = true;
        EarnedAchievements = 0;
//        string service = "api/v1/fan/me/Achievements?type=VIRTUALTOUR?";
        string service = "api/v1/fan/me/Achievements?";
        string url = string.Format("{0}?language={1}", service, Authentication.AzureServices.MainLanguage);
        while (needRequest) {            
            yield return Authentication.AzureServices.AwaitRequestGet(url, (res) => {
                if (res != "null")
                {
                    Debug.LogError(">>> MY achievements " + res);
                    Hashtable myachievements = JSON.JsonDecode(res) as Hashtable;
                    if (myachievements != null)
                    {
                        ArrayList myresults = myachievements["Results"] as ArrayList;
                        foreach (Hashtable ele in myresults)
                        {
                            string guid = ele["IdAchievement"] as string;
                            int level = (int)ele["Level"];
                            string aux = guid + "|" + level;
                            if (Achievements.ContainsKey(aux))
                            {
                                Achievement myvg = (Achievement)Achievements[aux];
                                myvg.earned = true;
                                EarnedAchievements++;
                                if (refresh) {
                                    Debug.LogError(">>>>>>>>>> RETO RECIEN GANADO " + aux);
                                }
                            }
                        }
                        needRequest = false;
                        if (myachievements.ContainsKey("HasMoreResults")) {
                            needRequest = (bool)myachievements["HasMoreResults"];
                            //Pedimos la siguiente pagina.
                            if(needRequest)
                                url = string.Format("{0}?language={1}&ct={2}", service, Authentication.AzureServices.MainLanguage, myachievements["ContinuationToken"] as string);
                        }
                    }
                }
                else
                    needRequest=false;
            });
        }
        
    }

    public void SedAction(string guid ) {
        Hashtable hs = new Hashtable();
        hs.Add("ActionId", guid );
        hs.Add("ClientId", Authentication.IDClient);
        Authentication.AzureServices.RequestPostJSON( "api/v1/useractions", hs, (res) => {
            Debug.LogError(">>>>>>>>>> " + res);
        });
    }
}