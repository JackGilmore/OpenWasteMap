using System;
using System.Collections.Generic;

namespace OpenWasteMapUK.Models
{
    // TODO: Stick this stuff in a database at some point
    public static class TagMappings
    {
        public static Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Aerosols","recycling:aerosol_cans"},
            { "Aluminium", "recycling:aluminium"},
            { "Batteries","recycling:batteries"},
            { "Books", "recycling:books"},
            { "Cans","recycling:cans"},
            { "Car batteries","recycling:car_batteries"},
            { "Cardboard","recycling:cardboard"},
            { "Cartons","recycling:cartons"},
            { "Ceramics","recycling:ceramics"},
            { "Clothes","recycling:clothes"},
            { "Cooking oil","recycling:cooking_oil"},
            { "Electrical appliances","recycling:electrical_appliances"},
            { "Electrical items","recycling:electrical_items"},
            { "Engine oil","recycling:engine_oil"},
            { "Fluorescent tubes","recycling:fluorescent_tubes"},
            { "Garden waste","recycling:garden_waste"},
            { "Glass","recycling:glass"},
            { "Glass bottles","recycling:glass_bottles"},
            { "Hazardous waste","recycling:hazardous_waste"},
            { "Magazines","recycling:magazines"},
            { "Newspapers","recycling:newspaper"},
            { "Paint","recycling:paint"},
            { "Paper","recycling:paper"},
            { "Paper packaging","recycling:paper_packaging"},
            { "Plasterboard","recycling:plasterboard"},
            { "Plastic bottles","recycling:plastic_bottles"},
            { "Residual waste","recycling:residual_waste"},
            { "Scrap metal","recycling:scrap_metal"},
            { "Small appliances","recycling:small_appliances;recycling:small_appliance"},
            { "Tyres","recycling:tyres"},
            { "White goods","recycling:white_goods;recycling:fridge_and_freezer"},
            { "Wood","recycling:wood"},
            { "Green waste","recycling:green_waste;recycling:garden_waste"},
            { "Plastics","recycling:plastic"},
            { "Gas canisters","recycling:gas_bottles;recycling:gas_cylinders"},
            { "Low energy bulbs","recycling:low_energy_bulbs"},
            { "Rubble","recycling:rubble"},
            { "Small electrical appliances","recycling:small_electrical_appliances"},
            { "General waste","recycling:waste"},
            { "Textiles","recycling:textiles"},
            { "Polystyrene","recycling:polystyrene"},
            { "Styrofoam","recycling:styrofoam"},
            { "Wood waste","recycling:wood;recycling:wood_waste"},
            { "Polystyrene foam","recycling:polystyrene_foam"},
            { "Earth","recycling:earth;recycling:soil"},
            { "Furniture","recycling:furniture"},
            { "Animal waste","recycling:animal_waste"}
        };
    }
}
