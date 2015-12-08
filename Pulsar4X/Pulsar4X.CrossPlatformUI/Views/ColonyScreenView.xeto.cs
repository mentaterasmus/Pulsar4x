﻿using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;
using Pulsar4X.ViewModel;

namespace Pulsar4X.CrossPlatformUI.Views
{
    public class ColonyScreenView : Panel
    {

        protected ComboBox ColonySelection { get; set; }
        protected GridView FacDataGrid { get; set; }
        protected GridView PopDataGrid { get; set; }
        protected JobAbilityView JobAbilityView { get; set; }
        private ColonyScreenVM _vm { get; set; }

        protected ColonyScreenView()
        {
            XamlReader.Load(this);
        }

        public ColonyScreenView(GameVM gameVM) :this()
        {
            ColonySelection.DataStore = gameVM.Colonys.Cast<object>();

            ColonySelection.ItemTextBinding = Binding.Property((KeyValuePair<Guid, string> r) => r.Value);
            ColonySelection.ItemKeyBinding = Binding.Property((KeyValuePair<Guid, string> r) => r.Key).Convert(r => r.ToString());

            

            //ColonySelection.SelectedKeyBinding.Convert(r => new Guid(r), g => g.ToString()).BindDataContext((GameVM m) => m.SetColonyScreen);

            //_vm = ColonySelection.SelectedValue;

            FacDataGrid.DataContext = _vm.Facilities;
            PopDataGrid.DataContext = _vm.Species;
            //JobAbilityView = new JobAbilityView(_vm.ConstructionAbilityVM,);

        }

    }
}