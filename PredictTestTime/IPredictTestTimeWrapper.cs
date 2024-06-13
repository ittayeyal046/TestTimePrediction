// <copyright file="IPredictTestTimeWrapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PredictTestTimeWrapper;

public interface IPredictTestTimeWrapper
{
    /// <summary>
    /// Predicts the
    /// </summary>
    /// <param name="parametersDictionary">The parameters' dictionary.</param>
    /// <returns>A TimeSpan.</returns>
    Task<TimeSpan> Predict(IDictionary<string, string> parametersDictionary);
}