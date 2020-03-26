namespace Genometric.TVQ.API.Analysis.Clustering
{
    public enum DistanceMetric
    {
        /// <summary>
        /// Canberra Distance, a weighted version of the L1-norm of the difference.
        /// </summary>
        Canberra,

        /// <summary>
        /// Chebyshev Distance, i.e. the Infinity-norm of the difference.
        /// </summary>
        Chebyshev,

        /// <summary>
        /// Cosine Distance, representing the angular distance while ignoring the scale.
        /// </summary>
        Cosine,

        /// <summary>
        /// Euclidean Distance, i.e. the L2-norm of the difference.
        /// </summary>
        Euclidean,

        /// <summary>
        /// Hamming Distance, i.e. the number of positions that have different values in the vectors.
        /// </summary>
        Hamming,

        /// <summary>
        /// Jaccard distance, i.e. 1 - the Jaccard index.
        /// </summary>
        Jaccard,

        /// <summary>
        /// Mean-Absolute Error (MAE), i.e. the normalized L1-norm (Manhattan) of the difference.
        /// </summary>
        MAE,

        /// <summary>
        /// Manhattan Distance, i.e. the L1-norm of the difference.
        /// </summary>
        Manhattan,

        /// <summary>
        /// Mean-Squared Error (MSE), i.e. the normalized squared L2-norm (Euclidean) of the difference.
        /// </summary>
        MSE,

        /// <summary>
        /// Pearson's distance, i.e. 1 - the person correlation coefficient.
        /// </summary>
        Pearson,

        /// <summary>
        /// Sum of Absolute Difference (SAD), i.e. the L1-norm (Manhattan) of the difference.
        /// </summary>
        SAD,

        /// <summary>
        /// Sum of Squared Difference (SSD), i.e. the squared L2-norm (Euclidean) of the difference.
        /// </summary>
        SSD
    }
}
