﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket
{
    public class Player
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string position { get; set; }
        public float height { get; set; }
        public float weight { get; set; }
        public int age { get; set; }
        public string city { get; set; }
        public string? team { get; set; }
        public Player(string _name, string _surname, string pos, float ht, float wt, int _age, string _city, string? _team)
        {
            name = _name;
            surname = _surname;
            position = pos;
            height = ht;
            weight = wt;
            age = _age;
            city = _city;
            team = _team;
        }
        public Player() { }

        public void UpdateData(Player other)
        {
            this.name = other.name;
            this.surname = other.surname;
            this.position = other.position;
            this.height = other.height;
            this.weight = other.weight;
            this.age = other.age;
            this.city = other.city;
            this.team = other.team;
        }

        public override string ToString()
        {
            return string.Format("Name: {0} Surname: {1} Position: {2} Height: {3} Weight: {4} Age: {5} City: {6} Team: {7}",
                name, surname, position, height, weight, age, city, team);
        }
    }
}