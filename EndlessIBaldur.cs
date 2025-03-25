using Modding;
using System.Collections.Generic;
using UnityEngine;
using Satchel.BetterMenus;
using System.Threading.Tasks;

namespace EndlessIBaldur {
    public class EndlessIBaldur: Mod, ITogglableMod, ICustomMenuMod, IGlobalSettings<GlobalSettings> {
        new public string GetName() => "EndlessIBaldur";
        public override string GetVersion() => "1.0.0.0";

        private Menu MenuRef;
        public static GlobalSettings gs = new();

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {
            On.GameManager.OnNextLevelReady += lateSceneChange;
        }

        public void Unload() {
            On.GameManager.OnNextLevelReady -= lateSceneChange;
        }
        
        private void lateSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self) {
            orig(self);
            if(self.sceneName == "Crossroads_11_alt") {
                TransitionPoint[] transitions = GameObject.FindObjectsOfType<TransitionPoint>();
                foreach(var t in transitions) {
                    if(t.gameObject.name == "left1") {
                        t.targetScene = "Crossroads_11_alt";
                    }
                }
                HealthManager[] hm = GameObject.FindObjectsOfType<HealthManager>(true);
                foreach(HealthManager h in hm) {
                    if(h.gameObject.name == "Blocker") {
                        h.isDead = false;
                        h.gameObject.SetActive(true);
                    }
                }
                setMpHp();
            }
        }

        public async void setMpHp() {
            float startTime = Time.time;
            while(Time.time - startTime < 1) {
                await Task.Yield();
            }
            HeroController hero = HeroController.instance;
            hero.SetMPCharge(gs.startMp);
            int health = PlayerData.instance.health;
            if(health > gs.startHp)
                hero.TakeHealth(PlayerData.instance.health - gs.startHp);
            else if(health < gs.startHp)
                hero.AddHealth(gs.startHp - PlayerData.instance.health);
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? modtoggledelegates) {
            MenuRef ??= new Menu(
                name: "Endless iBaldur",
                elements: new Element[]
                {
                    new CustomSlider(
                        name: "Starting Health",
                        storeValue: val => {
                            gs.startHp = (int)val;
                        },
                        loadValue : () => gs.startHp,
                        minValue: 1,
                        maxValue: 5,
                        wholeNumbers: true
                        ),
                    new CustomSlider(
                        name: "Starting Soul",
                        storeValue: val => {
                            gs.startMp = (int)val;
                        },
                        loadValue: () => gs.startMp,
                        minValue: 0,
                        maxValue: 99,
                        wholeNumbers: true
                        )
                }
            );

            return MenuRef.GetMenuScreen(modListMenu);
        }

        public bool ToggleButtonInsideMenu {
            get;
        }

        public void OnLoadGlobal(GlobalSettings s) {
            gs = s;
        }

        public GlobalSettings OnSaveGlobal() {
            return gs;
        }
    }

    public class GlobalSettings {
        public int startHp = 3;
        public int startMp = 66;
    }
}