using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAChatBot.Luis
{
    public static class LuisResponseExtensions
    {
        public static Intent TopIntent(this LuisResponse luisResponse)
        {
            if (!luisResponse.Intents.Any())
            {
                return null;
            }

            Intent winner = luisResponse.Intents.First();
            for (int i = 1; i < luisResponse.Intents.Count; ++i)
            {
                if (luisResponse.Intents[i].Score > winner.Score)
                {
                    winner = luisResponse.Intents[i];
                }
            }

            return winner;
        }

        public static bool TryFindType(this LuisResponse luisResponse, string typeName, out Entity recommendation)
        {
            try
            {
                recommendation = null;

                if (luisResponse == null || typeName == null || luisResponse.Entities == null)
                {
                    return false;
                }

                foreach (var entityRecommendation in luisResponse.Entities)
                {
                    if (String.Compare(entityRecommendation.Type, typeName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        recommendation = entityRecommendation;
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                recommendation = null;
                return false;
            }
            return false;
        }

        public static List<Entity> FindType(this LuisResponse luisResponse, string typeName)
        {
            List<Entity> entities = new List<Entity>();

            try
            {
                if (luisResponse == null || typeName == null || luisResponse.Entities == null)
                {
                    return null;
                }

                foreach (var entityRecommendation in luisResponse.Entities)
                {
                    if (String.Compare(entityRecommendation.Type, typeName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        entities.Add(entityRecommendation);
                    }
                }
            }
            catch (Exception)
            {
                entities = null;
            }
            return entities;
        }

        public static bool TryFindIntent(this LuisResponse luisResponse, string intentName, out Intent intent)
        {
            try
            {
                intent = null;

                if (luisResponse == null || intentName == null || luisResponse.Intents == null)
                {
                    return false;
                }

                foreach (var luisIntent in luisResponse.Intents)
                {
                    if (String.Compare(luisIntent.Name, intentName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        intent = luisIntent;
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                intent = null;
                return false;
            }
            return false;
        }

        public static bool IsTopIntent(this LuisResponse luisResponse, string intentName, out Intent intent)
        {
            try
            {
                intent = null;

                if (luisResponse == null || intentName == null || luisResponse.Intents == null || luisResponse.Intents.Count == 0)
                {
                    return false;
                }

                var firstIntent = luisResponse.Intents.First();

                if (String.Compare(firstIntent.Name, intentName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    intent = firstIntent;
                    return true;
                }
            }
            catch (Exception)
            {
                intent = null;
                return false;
            }
            return false;
        }
    }
}