using System;
using UnityEngine;

namespace Major {
    public class Debug {

        [Command("foo")]
        public void foo() {

        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute {
        public string alias;

        public CommandAttribute() {

        }

        public CommandAttribute(string alias) {
            this.alias = alias;
        }
    };
}