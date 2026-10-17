namespace LastHaven.Editor
{
    /// <summary>
    /// Yapı prefab'ının yer tutucu görünümü (GDD §18): barikat kutu, taret silindir gövde + dönen küp kafa, diken tuzağı yassı küp.
    /// </summary>
    public enum BuildableShape
    {
        /// <summary>Ayak izi × yükseklik küp ("Visual/Body").</summary>
        Box,

        /// <summary>Silindir gövde + dönen kafa, nişan pivotu ve namlu (<see cref="TurretPrefab"/>).</summary>
        Turret,

        /// <summary>
        /// Yassı plaka + dikenler; üstünden geçilir: kökte trigger kutusu, NavMeshObstacle yok (<see cref="SpikeTrapPrefab"/>).
        /// </summary>
        SpikeTrap,
    }
}
