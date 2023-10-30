namespace WebAppForMORecSys.Cache
{
    /// <summary>
    /// Used for sanity check question on item
    /// </summary>
    public static class SanityCheckCache
    {
        /// <summary>
        /// Randomly selected items for sanity questions are 
        /// </summary>
        public static Dictionary<Tuple<int, int>, int> Cache = new Dictionary<Tuple<int, int>, int>();

        /// <summary>
        /// Adds new item for sanity check question to the cache
        /// </summary>
        /// <param name="userID">ID of user</param>
        /// <param name="questionID">ID of sanity check question</param>
        /// <param name="itemID">ID of item selected for sanity check</param>
        public static void Add(int userID, int questionID, int itemID)
        {
            var key = new Tuple<int, int>(userID, questionID);
            lock (Cache)
            {                
                if (Cache.ContainsKey(key))
                {
                    Cache[key] = itemID;
                }
                else 
                {
                    Cache.Add(key, itemID);
                }
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="userID">ID of user</param>
        /// <param name="questionID">ID of sanity check question</param>
        /// <returns>ID of item selected for sanity check question</returns>
        public static int? Get(int userID, int questionID)
        {
            var key = new Tuple<int, int>(userID, questionID);
            if (Cache.ContainsKey(key))            
                return Cache[key];
            return null;
        }
    }
}
