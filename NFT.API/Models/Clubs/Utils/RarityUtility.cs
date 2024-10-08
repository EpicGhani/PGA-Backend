using NFT.API.Models.Clubs.Interface;

namespace NFT.API.Models.Clubs.Utils
{
    public static class RarityUtility
    {
        /// <summary>
        /// Roll a value between 0 and <see cref="total"/>, affected by rarity.
        /// </summary>
        /// <param name="rarity">
        /// The expected rarity of an object that will affect the threshold of the value to be rolled.
        /// </param>
        /// <param name="total">
        /// The highest possible value that can be obtained.
        /// </param>
        /// <returns>
        /// A value multiplied by the modifier calculated by depending on rarity, relative to the <see cref="total"/>.
        /// </returns>
        public static int RollValue(this Rarity rarity, int total = 100)
        {
            Random random = new Random();
            var modifier = rarity switch
            {
                Rarity.Champion => (float)(random.NextDouble() * (1.0f - .875f) + .875f),
                Rarity.Master => (float)(random.NextDouble() * (.875f - .75f) + .75f),
                Rarity.Advanced => (float)(random.NextDouble() * (.75f - .625f) + .625f),
                Rarity.Intermediate => (float)(random.NextDouble() * (.625f - .5f) + .625f),
                Rarity.Entry => (float)(random.NextDouble() * (.5f - 0f) + .5f),
                _ => (float)(random.NextDouble() * (.5f - 0f) + .5f),
            };

            return (int)(modifier * total);
        }

        /// <summary>
        /// Convert an integer value into a Rarity, calculated as a percentage relative to <see cref="total"/>/
        /// </summary>
        /// <param name="value">
        /// The value to be converted into a percentage used as the dividend.
        /// </param>
        /// <param name="total">
        /// The max value of a Rarity, used as the divisor.
        /// </param>
        /// <returns>
        /// A Rarity value describing the percentage between <see cref="value"/> and <see cref="total"/>.
        /// </returns>
        public static Rarity ToRarity(int value, int total = 100)
        {
            var percent = (float)value / total;
            return ToRarity(percent);
        }

        /// <summary>
        /// Convert a percentage into a Rarity.
        /// </summary>
        /// <param name="percent">
        /// The percent value to be calculated into a Rarity value.
        /// </param>
        /// <returns>
        /// <inheritdoc cref="ToRarity(int,int)"/>
        /// </returns>
        public static Rarity ToRarity(float percent)
        {
            return percent switch
            {
                > .875f => Rarity.Champion,
                > .75f => Rarity.Master,
                > .625f => Rarity.Advanced,
                > .5f => Rarity.Intermediate,
                _ => Rarity.Entry,
            };
        }

        /// <summary>
        /// Convert a Rarity to a percent value, in float.
        /// </summary>
        /// <param name="rarity">
        /// The Rarity to be converted into a percent value.
        /// </param>
        /// <returns>
        /// The max threshold of a Rarity in a percent value.
        /// </returns>
        public static float ToPercent(this Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Champion => 1.0f,
                Rarity.Master => .875f,
                Rarity.Advanced => .75f,
                Rarity.Intermediate => .625f,
                Rarity.Entry => .5f,
                _ => .5f,
            };
        }
    }
}
