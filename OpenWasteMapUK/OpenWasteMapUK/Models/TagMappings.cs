using System;
using System.Collections.Generic;

namespace OpenWasteMapUK.Models
{
    // TODO: Stick this stuff in a database at some point
    public static class TagMappings
    {
        public static Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Aerosols", "recycling:aerosol_cans" },
            { "Aluminium", "recycling:aluminium" },
            { "Animal waste", "recycling:animal_waste" },
            { "Batteries", "recycling:batteries" },
            { "Bicycles", "recycling:bicycles" },
            { "Books", "recycling:books" },
            { "Cans", "recycling:cans" },
            { "Car batteries", "recycling:car_batteries" },
            { "Cardboard", "recycling:cardboard" },
            { "Cartons", "recycling:cartons" },
            { "Ceramics", "recycling:ceramics" },
            { "Clothes", "recycling:clothes" },
            { "Cooking oil", "recycling:cooking_oil" },
            { "Earth", "recycling:earth;recycling:soil" },
            { "Electrical appliances", "recycling:electrical_appliances" },
            { "Electrical items", "recycling:electrical_items" },
            { "Engine oil", "recycling:engine_oil" },
            { "Fluorescent tubes", "recycling:fluorescent_tubes" },
            { "Furniture", "recycling:furniture" },
            { "Gas canisters", "recycling:gas_bottles;recycling:gas_cylinders" },
            { "General waste", "recycling:waste" },
            { "Glass", "recycling:glass" },
            { "Glass bottles", "recycling:glass_bottles" },
            { "Green waste", "recycling:green_waste;recycling:garden_waste" },
            { "Hazardous waste", "recycling:hazardous_waste" },
            { "Low energy bulbs", "recycling:low_energy_bulbs" },
            { "Magazines", "recycling:magazines" },
            { "Newspapers", "recycling:newspaper" },
            { "Paint", "recycling:paint" },
            { "Paper", "recycling:paper" },
            { "Paper packaging", "recycling:paper_packaging" },
            { "Plasterboard", "recycling:plasterboard" },
            { "Plastics", "recycling:plastic" },
            { "Plastic bottles", "recycling:plastic_bottles" },
            { "Polystyrene", "recycling:polystyrene" },
            { "Polystyrene foam", "recycling:polystyrene_foam" },
            { "Residual waste", "recycling:residual_waste" },
            { "Rubble", "recycling:rubble" },
            { "Scrap metal", "recycling:scrap_metal" },
            { "Small appliances", "recycling:small_appliances;recycling:small_appliance" },
            { "Small electrical appliances", "recycling:small_electrical_appliances" },
            { "Styrofoam", "recycling:styrofoam" },
            { "Textiles", "recycling:textiles" },
            { "Tyres", "recycling:tyres" },
            { "Turf", "recycling:turf" },
            { "White goods", "recycling:white_goods;recycling:fridge_and_freezer" },
            { "Wood", "recycling:wood" },
            { "Wood waste", "recycling:wood;recycling:wood_waste" }
        };
    }
}
