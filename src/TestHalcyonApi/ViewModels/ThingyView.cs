﻿using Halcyon.HAL.Attributes;
using HateoasTest.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TestHalcyonApi.Controllers;
using TestHalcyonApi.Models;
using Threax.AspNetCore.Halcyon.Ext;
using Threax.AspNetCore.Halcyon.Ext.UIAttrs;

namespace TestHalcyonApi.ViewModels
{
    /// <summary>
    /// A simple test model.
    /// </summary>
    [HalModel]
    [HalSelfActionLink(ThingyController.Rels.Get, typeof(ThingyController))]
    [HalActionLink(ThingyController.Rels.Get, typeof(ThingyController))]
    [HalActionLink(ThingyController.Rels.Update, typeof(ThingyController))]
    [HalActionLink(ThingyController.Rels.Delete, typeof(ThingyController))]
    [HalActionLink(ThingyController.Rels.ListTestSubData, typeof(ThingyController))]
    [HalActionLink(SubThingyController.Rels.Add, typeof(SubThingyController))]
    [HalActionLink(ThingyController.Rels.AuthorizedProperties, typeof(ThingyController))]
    [HalActionLink(ThingyController.Rels.RoleProperties, typeof(ThingyController))]
    [DeclareHalLink(ThingyController.Rels.TestDeclareLinkToRel, typeof(ThingyController))]
    public class ThingyView : Thingy
    {
        public List<ComplexObject> ComplexObjects { get; set; }

        public bool ShowHidden { get; set; }

        private static readonly Expression<Func<ThingyView, bool>> PropertyToShowExpression = s => s.ShowHidden;
        [DisplayExpression(nameof(PropertyToShowExpression))]
        public int PropertyToShow { get; set; }

        //public int Counter { get; set; }

        //private static readonly Expression<Func<ThingyView, bool>> CounterTest = s => s.Counter > 5;
        //[DisplayExpression(nameof(CounterTest))]
        //public int ShowIfCounterBiggerThan5 { get; set; }

        //public String TextValue { get; set; }

        //private static readonly Expression<Func<ThingyView, bool>> ShowIfTextValueIsBeefTest = s => s.TextValue == "Beef";
        //[DisplayExpression(nameof(ShowIfTextValueIsBeefTest))]
        //public int ShowIfTextValueIsBeef { get; set; }
    }
}
