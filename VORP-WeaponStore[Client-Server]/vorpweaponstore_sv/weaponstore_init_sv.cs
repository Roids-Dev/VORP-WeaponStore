﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpweaponstore_sv
{
    public class weaponstore_init_sv : BaseScript
    {
        public weaponstore_init_sv()
        {
            EventHandlers["vorpweaponstore:BuyWeapon"] += new Action<Player, int>(buyItems);
            EventHandlers["vorpweaponstore:RestockAmmo"] += new Action<Player, int, double, string, int>(restockAmmo);
            EventHandlers["vorpweaponstore:BuyAmmoItem"] += new Action<Player, string, double>(BuyAmmoItem);
            EventHandlers["vorpweaponstore:DeleteAmmoBox"] += new Action<Player, string>(delItems);
            RegisterUsableItems();
        }

        private async Task RegisterUsableItems()
        {
            await Delay(3000);

            foreach (JObject content in LoadConfig.Config["AmmoGivesOnUse"].Children<JObject>())
            {
                foreach (JProperty ammoboxes in content.Properties())
                {
                    TriggerEvent("vorpCore:registerUsableItem", ammoboxes.Name, new Action<dynamic>((data) =>
                    {
                        PlayerList pl = new PlayerList();
                        Player p = pl[data.source];
                        p.TriggerEvent("vorp_weaponstore:useAmmoItem", ammoboxes.Name);
                    }));
                    await Delay(800);
                }
            }
        }

        private void restockAmmo([FromSource]Player source, int weaponId, double cost, string typeAmmo, int quantity)
        {
            int _source = int.Parse(source.Handle);

            string sid = "steam:" + source.Identifiers["steam"];

            Debug.WriteLine(weaponId.ToString());
            Debug.WriteLine(cost.ToString());
            Debug.WriteLine(typeAmmo.ToString());
            Debug.WriteLine(quantity.ToString());

            TriggerEvent("vorp:getCharacter", _source, new Action<dynamic>((user) =>
            {
                double money = user.money;
                if (cost <= money)
                {
                    TriggerEvent("vorp:removeMoney", _source, 0, cost);
                    TriggerEvent("vorpCore:addBullets", _source, weaponId, typeAmmo, quantity);
                }
                else
                {
                    source.TriggerEvent("vorp:TipRight", LoadConfig.Langs["NoMoney"], 4000);
                }

            }));
        }

        private void BuyAmmoItem([FromSource]Player source, string name, double cost)
        {
            int _source = int.Parse(source.Handle);

            string sid = "steam:" + source.Identifiers["steam"];

            TriggerEvent("vorp:getCharacter", _source, new Action<dynamic>((user) =>
            {
                double money = user.money;
                if (cost <= money)
                {

                    TriggerEvent("vorp:removeMoney", _source, 0, cost);
                    TriggerEvent("vorpCore:addItem", _source, name, 1);
                    source.TriggerEvent("vorp:TipRight", string.Format(LoadConfig.Langs[name.ToLower()], cost.ToString()), 3000);
                }
                else
                {
                    source.TriggerEvent("vorp:TipRight", LoadConfig.Langs["NoMoney"], 4000);
                }

            }));
        }

        private void buyItems([FromSource]Player source, int index)
        {
            int _source = int.Parse(source.Handle);

            string sid = "steam:" + source.Identifiers["steam"];

            string weaponName = LoadConfig.Config["Weapons"][index]["Name"].ToObject<string>();
            string weaponHash = LoadConfig.Config["Weapons"][index]["HashName"].ToObject<string>();
            double cost = LoadConfig.Config["Weapons"][index]["Price"].ToObject<double>();

            TriggerEvent("vorp:getCharacter", _source, new Action<dynamic>((user) =>
            {
                double money = user.money;
                if (cost <= money)
                {
                    Dictionary<string, int> ammoaux = new Dictionary<string, int>();
                    TriggerEvent("vorp:removeMoney", _source, 0, cost);
                    TriggerEvent("vorpCore:registerWeapon", _source, weaponHash, ammoaux, ammoaux);
                    source.TriggerEvent("vorp:TipRight", string.Format(LoadConfig.Langs["YouBoughtWeapon"], weaponName, cost.ToString()), 4000);
                }
                else
                {
                    source.TriggerEvent("vorp:TipRight", LoadConfig.Langs["NoMoney"], 4000);
                }

            }));
        }

        private void delItems([FromSource]Player source, string item)
        {
            int _source = int.Parse(source.Handle);

            TriggerEvent("vorpCore:subItem", _source, item, 1);

        }
    }
}
