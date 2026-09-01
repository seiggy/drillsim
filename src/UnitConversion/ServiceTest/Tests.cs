using System;
using System.Net.Http.Headers;
using OSDC.UnitConversion.ModelShared;
using Parlot.Fluent;

namespace OSDC.UnitConversion.ServiceTest
{
    public class Tests
    {
        // testing outside Visual Studio requires using http port (https faces authentication issues both in console and on github)
        //private static string host = "https://dev.digiwells.no/";
        //private static string host = "http://localhost:5002/";
        private static string host = "http://localhost:8080/";
        //private static string host = "https://localhost:44368/";
        //private static string host = "http://localhost:54949/";
        private static HttpClient httpClient;
        private static Client nSwagClient;
        private static bool bypassTests = true;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            httpClient = new() { BaseAddress = new Uri(host + "UnitConversion/api/") };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            nSwagClient = new Client(httpClient.BaseAddress.ToString(), httpClient);
        }

        [Test]
        public async Task Test_UnitConversionSet_Get()
        {

            if (!bypassTests)
            {
                #region post a UnitConversionSet
                Guid guid = Guid.NewGuid();
                MetaInfo metaInfo = new() { ID = guid };
                UnitConversionSet unitConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitConversionSet for Get",
                    QuantityUnitConversionList = []
                };
                try
                {
                    await nSwagClient.PostUnitConversionSetAsync(unitConversionSet);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post given UnitConversionSet\n" + ex.Message);
                }
                #endregion

                #region GetAllUnitConversionSetId
                List<Guid> idList = [];
                try
                {
                    idList = (List<Guid>)await nSwagClient.GetAllUnitConversionSetIdAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get all UnitConversionSet ids\n" + ex.Message);
                }
                Assert.That(idList, Is.Not.Null);
                Assert.That(idList, Does.Contain(guid));
                #endregion

                #region GetAllUnitConversionSetMetaInfo
                List<MetaInfo> metaInfoList = [];
                try
                {
                    metaInfoList = (List<MetaInfo>)await nSwagClient.GetAllUnitConversionSetMetaInfoAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get all UnitConversionSet metainfos\n" + ex.Message);
                }
                Assert.That(metaInfoList, Is.Not.Null);
                IEnumerable<MetaInfo> metaInfoList2 =
                    from elt in metaInfoList
                    where elt.ID == guid
                    select elt;
                Assert.That(metaInfoList2, Is.Not.Null);
                Assert.That(metaInfoList2, Is.Not.Empty);
                #endregion

                #region GetAllUnitConversionSetById
                UnitConversionSet? unitConversionSet2 = null;
                try
                {
                    unitConversionSet2 = await nSwagClient.GetUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get UnitConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitConversionSet2, Is.Not.Null);
                Assert.That(unitConversionSet2.MetaInfo.ID, Is.EqualTo(guid));
                #endregion


                #region GetAllUnitConversionSet
                List<UnitConversionSet> unitConversionSetList = [];
                try
                {
                    unitConversionSetList = (List<UnitConversionSet>)await nSwagClient.GetAllUnitConversionSetAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the list of UnitConversionSet\n" + ex.Message);
                }
                Assert.That(unitConversionSetList, Is.Not.Null);
                IEnumerable<UnitConversionSet> unitConversionSetList2 =
                    from elt in unitConversionSetList
                    where elt.MetaInfo.ID == guid
                    select elt;
                Assert.That(unitConversionSetList2, Is.Not.Null);
                Assert.That(unitConversionSetList2, Is.Not.Empty);
                #endregion

                #region finally delete the new ID
                unitConversionSet2 = null;
                try
                {
                    await nSwagClient.DeleteUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitConversionSet of given Id\n" + ex.Message);
                }
                try
                {
                    unitConversionSet2 = await nSwagClient.GetUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get UnitConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitConversionSet2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitConversionSet_Post()
        {
            if (!bypassTests)
            {
                #region trying to post an empty guid
                Guid guid = Guid.Empty;
                MetaInfo metaInfo = new() { ID = guid };
                UnitConversionSet unitConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitConversionSet for Post",
                    QuantityUnitConversionList = []
                };
                UnitConversionSet? unitConversionSet2 = null;
                try
                {
                    await nSwagClient.PostUnitConversionSetAsync(unitConversionSet);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(400));
                    TestContext.WriteLine("Impossible to Post UnitConversionSet with empty Guid\n" + ex.Message);
                }
                try
                {
                    unitConversionSet2 = await nSwagClient.GetUnitConversionSetByIdAsync(Guid.Empty);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(400));
                    TestContext.WriteLine("Impossible to Get UnitConversionSet identified by an empty Guid\n" + ex.Message);
                }
                Assert.That(unitConversionSet2, Is.Null);
                #endregion

                #region post some corrupted data
                // post data with missing input that fails the calculation process
                #endregion

                #region posting a new ID in a valid state
                guid = Guid.NewGuid();
                metaInfo = new MetaInfo { ID = guid };
                unitConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitConversionSet for Post",
                    QuantityUnitConversionList = []
                };
                try
                {
                    await nSwagClient.PostUnitConversionSetAsync(unitConversionSet);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post UnitConversionSet although it is in a valid state\n" + ex.Message);
                }
                try
                {
                    unitConversionSet2 = await nSwagClient.GetUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the UnitConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitConversionSet2, Is.Not.Null);
                Assert.That(unitConversionSet2.MetaInfo, Is.Not.Null);
                Assert.That(unitConversionSet2.MetaInfo.ID, Is.EqualTo(guid));
                Assert.That(unitConversionSet2.Name, Is.EqualTo(unitConversionSet.Name));
                #endregion

                #region trying to repost the same ID
                unitConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitConversionSet for Post with modified name",
                    QuantityUnitConversionList = []
                };
                bool conflict = false;
                try
                {
                    await nSwagClient.PostUnitConversionSetAsync(unitConversionSet);
                }
                catch (ApiException ex)
                {
                    conflict = true;
                    Assert.That(ex.StatusCode, Is.EqualTo(409));
                    TestContext.WriteLine("Impossible to Post existing UnitConversionSet\n" + ex.Message);
                }
                Assert.That(conflict, Is.True);
                #endregion

                #region finally delete the new ID
                unitConversionSet2 = null;
                try
                {
                    await nSwagClient.DeleteUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitConversionSet of given Id\n" + ex.Message);
                }
                try
                {
                    unitConversionSet2 = await nSwagClient.GetUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get deleted UnitConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitConversionSet2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitConversionSet_Put()
        {
            if (!bypassTests)
            {
                #region posting a new ID
                Guid guid = Guid.NewGuid();
                MetaInfo metaInfo = new MetaInfo { ID = guid };
                UnitConversionSet unitConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitConversionSet for Put",
                    QuantityUnitConversionList = []
                };
                UnitConversionSet? unitConversionSet2 = null;
                try
                {
                    await nSwagClient.PostUnitConversionSetAsync(unitConversionSet);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post UnitConversionSet\n" + ex.Message);
                }
                try
                {
                    unitConversionSet2 = await nSwagClient.GetUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the UnitConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitConversionSet2, Is.Not.Null);
                Assert.That(unitConversionSet2.MetaInfo, Is.Not.Null);
                Assert.That(unitConversionSet2.MetaInfo.ID, Is.EqualTo(guid));
                Assert.That(unitConversionSet2.Name, Is.EqualTo(unitConversionSet.Name));
                #endregion

                #region updating the new Id
                unitConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitConversionSet for Put with modified name",
                    QuantityUnitConversionList = []
                };
                try
                {
                    await nSwagClient.PutUnitConversionSetByIdAsync(unitConversionSet.MetaInfo.ID, unitConversionSet);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Put UnitConversionSet of given Id\n" + ex.Message);
                }
                try
                {
                    unitConversionSet2 = await nSwagClient.GetUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the updated UnitConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitConversionSet2, Is.Not.Null);
                Assert.That(unitConversionSet2.MetaInfo, Is.Not.Null);
                Assert.That(unitConversionSet2.MetaInfo.ID, Is.EqualTo(guid));
                Assert.That(unitConversionSet2.Name, Is.EqualTo(unitConversionSet.Name));
                #endregion

                #region finally delete the new ID
                unitConversionSet2 = null;
                try
                {
                    await nSwagClient.DeleteUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitConversionSet of given Id\n" + ex.Message);
                }
                try
                {
                    unitConversionSet2 = await nSwagClient.GetUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get deleted UnitConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitConversionSet2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitConversionSet_Delete()
        {
            if (!bypassTests)
            {
                #region posting a new ID
                Guid guid = Guid.NewGuid();
                MetaInfo metaInfo = new MetaInfo { ID = guid };
                UnitConversionSet unitConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitConversionSet for Delete",
                    QuantityUnitConversionList = []
                };
                UnitConversionSet? unitConversionSet2 = null;
                try
                {
                    await nSwagClient.PostUnitConversionSetAsync(unitConversionSet);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post UnitConversionSet\n" + ex.Message);
                }
                try
                {
                    unitConversionSet2 = await nSwagClient.GetUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the UnitConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitConversionSet2, Is.Not.Null);
                Assert.That(unitConversionSet2.MetaInfo, Is.Not.Null);
                Assert.That(unitConversionSet2.MetaInfo.ID, Is.EqualTo(guid));
                #endregion

                #region finally delete the new ID
                unitConversionSet2 = null;
                try
                {
                    await nSwagClient.DeleteUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitConversionSet of given Id\n" + ex.Message);
                }
                try
                {
                    unitConversionSet2 = await nSwagClient.GetUnitConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get deleted UnitConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitConversionSet2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitSystemConversionSet_Get()
        {
            if (!bypassTests)
            {
                #region post a UnitSystemConversionSet
                Guid guid = Guid.NewGuid();
                MetaInfo metaInfo = new MetaInfo { ID = guid };
                UnitSystemConversionSet unitSystemConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitSystemConversionSet for Get",
                    UnitSystemInID = new Guid("67e6faf9-8d2f-4071-badb-f8d1355017a4"),
                    UnitSystemOutID = new Guid("f8338e35-c548-4284-a2e7-61b94a7b4769"),
                    QuantityConversionList = []
                };
                try
                {
                    await nSwagClient.PostUnitSystemConversionSetAsync(unitSystemConversionSet);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post given UnitSystemConversionSet\n" + ex.Message);
                }
                #endregion

                #region GetAllUnitSystemConversionSetId
                List<Guid> idList = [];
                try
                {
                    idList = (List<Guid>)await nSwagClient.GetAllUnitSystemConversionSetIdAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get all UnitSystemConversionSet ids\n" + ex.Message);
                }
                Assert.That(idList, Is.Not.Null);
                Assert.That(idList, Does.Contain(guid));
                #endregion

                #region GetAllUnitSystemConversionSetMetaInfo
                List<MetaInfo> metaInfoList = [];
                try
                {
                    metaInfoList = (List<MetaInfo>)await nSwagClient.GetAllUnitSystemConversionSetMetaInfoAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get all UnitSystemConversionSet metainfos\n" + ex.Message);
                }
                Assert.That(metaInfoList, Is.Not.Null);
                IEnumerable<MetaInfo> metaInfoList2 =
                    from elt in metaInfoList
                    where elt.ID == guid
                    select elt;
                Assert.That(metaInfoList2, Is.Not.Null);
                Assert.That(metaInfoList2, Is.Not.Empty);
                #endregion

                #region GetAllUnitSystemConversionSetById
                UnitSystemConversionSet? unitSystemConversionSet2 = null;
                try
                {
                    unitSystemConversionSet2 = await nSwagClient.GetUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSet2, Is.Not.Null);
                Assert.That(unitSystemConversionSet2.MetaInfo.ID, Is.EqualTo(guid));
                #endregion


                #region GetAllUnitSystemConversionSet
                List<UnitSystemConversionSet> unitSystemConversionSetList = [];
                try
                {
                    unitSystemConversionSetList = (List<UnitSystemConversionSet>)await nSwagClient.GetAllUnitSystemConversionSetAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the list of UnitSystemConversionSet\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSetList, Is.Not.Null);
                IEnumerable<UnitSystemConversionSet> unitSystemConversionSetList2 =
                    from elt in unitSystemConversionSetList
                    where elt.MetaInfo.ID == guid
                    select elt;
                Assert.That(unitSystemConversionSetList2, Is.Not.Null);
                Assert.That(unitSystemConversionSetList2, Is.Not.Empty);
                #endregion

                #region finally delete the new ID
                unitSystemConversionSet2 = null;
                try
                {
                    await nSwagClient.DeleteUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                try
                {
                    unitSystemConversionSet2 = await nSwagClient.GetUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSet2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitSystemConversionSet_Post()
        {
            if (!bypassTests)
            {
                #region trying to post an empty guid
                Guid guid = Guid.Empty;
                MetaInfo metaInfo = new MetaInfo { ID = guid };
                UnitSystemConversionSet unitSystemConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitSystemConversionSet for Post",
                    UnitSystemInID = new Guid("67e6faf9-8d2f-4071-badb-f8d1355017a4"),
                    UnitSystemOutID = new Guid("f8338e35-c548-4284-a2e7-61b94a7b4769"),
                    QuantityConversionList = []
                };
                UnitSystemConversionSet? unitSystemConversionSet2 = null;
                try
                {
                    await nSwagClient.PostUnitSystemConversionSetAsync(unitSystemConversionSet);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(400));
                    TestContext.WriteLine("Impossible to Post UnitSystemConversionSet with empty Guid\n" + ex.Message);
                }
                try
                {
                    unitSystemConversionSet2 = await nSwagClient.GetUnitSystemConversionSetByIdAsync(Guid.Empty);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(400));
                    TestContext.WriteLine("Impossible to Get UnitSystemConversionSet identified by an empty Guid\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSet2, Is.Null);
                #endregion

                #region post some corrupted data
                // post data with missing input that fails the calculation process
                #endregion

                #region posting a new ID in a valid state
                guid = Guid.NewGuid();
                metaInfo = new MetaInfo { ID = guid };
                unitSystemConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitSystemConversionSet for Post",
                    UnitSystemInID = new Guid("67e6faf9-8d2f-4071-badb-f8d1355017a4"),
                    UnitSystemOutID = new Guid("f8338e35-c548-4284-a2e7-61b94a7b4769"),
                    QuantityConversionList = []
                };
                try
                {
                    await nSwagClient.PostUnitSystemConversionSetAsync(unitSystemConversionSet);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post UnitSystemConversionSet although it is in a valid state\n" + ex.Message);
                }
                try
                {
                    unitSystemConversionSet2 = await nSwagClient.GetUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSet2, Is.Not.Null);
                Assert.That(unitSystemConversionSet2.MetaInfo, Is.Not.Null);
                Assert.That(unitSystemConversionSet2.MetaInfo.ID, Is.EqualTo(guid));
                Assert.That(unitSystemConversionSet2.Name, Is.EqualTo(unitSystemConversionSet.Name));
                #endregion

                #region trying to repost the same ID
                unitSystemConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitSystemConversionSet for Post with modified name",
                    UnitSystemInID = new Guid("67e6faf9-8d2f-4071-badb-f8d1355017a4"),
                    UnitSystemOutID = new Guid("f8338e35-c548-4284-a2e7-61b94a7b4769"),
                    QuantityConversionList = []
                };
                bool conflict = false;
                try
                {
                    await nSwagClient.PostUnitSystemConversionSetAsync(unitSystemConversionSet);
                }
                catch (ApiException ex)
                {
                    conflict = true;
                    Assert.That(ex.StatusCode, Is.EqualTo(409));
                    TestContext.WriteLine("Impossible to Post existing UnitSystemConversionSet\n" + ex.Message);
                }
                Assert.That(conflict, Is.True);
                #endregion

                #region finally delete the new ID
                unitSystemConversionSet2 = null;
                try
                {
                    await nSwagClient.DeleteUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                try
                {
                    unitSystemConversionSet2 = await nSwagClient.GetUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get deleted UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSet2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitSystemConversionSet_Put()
        {
            if (!bypassTests)
            {
                #region posting a new ID
                Guid guid = Guid.NewGuid();
                MetaInfo metaInfo = new MetaInfo { ID = guid };
                UnitSystemConversionSet unitSystemConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitSystemConversionSet for Put",
                    UnitSystemInID = new Guid("67e6faf9-8d2f-4071-badb-f8d1355017a4"),
                    UnitSystemOutID = new Guid("f8338e35-c548-4284-a2e7-61b94a7b4769"),
                    QuantityConversionList = []
                };
                UnitSystemConversionSet? unitSystemConversionSet2 = null;
                try
                {
                    await nSwagClient.PostUnitSystemConversionSetAsync(unitSystemConversionSet);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post UnitSystemConversionSet\n" + ex.Message);
                }
                try
                {
                    unitSystemConversionSet2 = await nSwagClient.GetUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSet2, Is.Not.Null);
                Assert.That(unitSystemConversionSet2.MetaInfo, Is.Not.Null);
                Assert.That(unitSystemConversionSet2.MetaInfo.ID, Is.EqualTo(guid));
                Assert.That(unitSystemConversionSet2.Name, Is.EqualTo(unitSystemConversionSet.Name));
                #endregion

                #region updating the new Id
                unitSystemConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitSystemConversionSet for Put with modified name",
                    UnitSystemInID = new Guid("67e6faf9-8d2f-4071-badb-f8d1355017a4"),
                    UnitSystemOutID = new Guid("f8338e35-c548-4284-a2e7-61b94a7b4769"),
                    QuantityConversionList = []
                };
                try
                {
                    await nSwagClient.PutUnitSystemConversionSetByIdAsync(unitSystemConversionSet.MetaInfo.ID, unitSystemConversionSet);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Put UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                try
                {
                    unitSystemConversionSet2 = await nSwagClient.GetUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the updated UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSet2, Is.Not.Null);
                Assert.That(unitSystemConversionSet2.MetaInfo, Is.Not.Null);
                Assert.That(unitSystemConversionSet2.MetaInfo.ID, Is.EqualTo(guid));
                Assert.That(unitSystemConversionSet2.Name, Is.EqualTo(unitSystemConversionSet.Name));
                #endregion

                #region finally delete the new ID
                unitSystemConversionSet2 = null;
                try
                {
                    await nSwagClient.DeleteUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                try
                {
                    unitSystemConversionSet2 = await nSwagClient.GetUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get deleted UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSet2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitSystemConversionSet_Delete()
        {
            if (!bypassTests)
            {
                #region posting a new ID
                Guid guid = Guid.NewGuid();
                MetaInfo metaInfo = new MetaInfo { ID = guid };
                UnitSystemConversionSet unitSystemConversionSet = new()
                {
                    MetaInfo = metaInfo,
                    Name = "My test UnitSystemConversionSet for Delete",
                    UnitSystemInID = new Guid("67e6faf9-8d2f-4071-badb-f8d1355017a4"),
                    UnitSystemOutID = new Guid("f8338e35-c548-4284-a2e7-61b94a7b4769"),
                    QuantityConversionList = []
                };
                UnitSystemConversionSet? unitSystemConversionSet2 = null;
                try
                {
                    await nSwagClient.PostUnitSystemConversionSetAsync(unitSystemConversionSet);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post UnitSystemConversionSet\n" + ex.Message);
                }
                try
                {
                    unitSystemConversionSet2 = await nSwagClient.GetUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSet2, Is.Not.Null);
                Assert.That(unitSystemConversionSet2.MetaInfo, Is.Not.Null);
                Assert.That(unitSystemConversionSet2.MetaInfo.ID, Is.EqualTo(guid));
                #endregion

                #region finally delete the new ID
                unitSystemConversionSet2 = null;
                try
                {
                    await nSwagClient.DeleteUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                try
                {
                    unitSystemConversionSet2 = await nSwagClient.GetUnitSystemConversionSetByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get deleted UnitSystemConversionSet of given Id\n" + ex.Message);
                }
                Assert.That(unitSystemConversionSet2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitSystem_Get()
        {
            if (bypassTests)
            {
                Assert.Ignore("Requires an explicitly started integration service and mutates its unit-system database.");
            }

            //test code to remove the default unit systems
            List<Guid> idList = [];
            try
            {
                idList = (List<Guid>)await nSwagClient.GetAllUnitSystemIdAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to Get all UnitSystem ids\n" + ex.Message);
            }
            List<UnitSystem> unitSystems = new List<UnitSystem>();
            foreach (Guid id in idList)
            {
                try
                {
                    var unitSystem = OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering.DrillingUnitSystem.Get(id);
                    if (unitSystem != null)
                    {
                        await nSwagClient.DeleteUnitSystemByIdAsync(id);
                    }

                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitSystem of given Id\n" + ex.Message);
                }
            }
            if (!bypassTests)
            {
                #region post a UnitSystem
                Guid guid = Guid.NewGuid();
                UnitSystem unitSystem = new()
                {
                    ID = guid,
                    Name = "My test UnitSystem for Get",
                    Description = "Test",
                    IsDefault = false,
                    IsSI = false,
                    Choices = new Dictionary<string, string>()
                };
                try
                {
                    await nSwagClient.PostUnitSystemAsync(unitSystem);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post given UnitSystem\n" + ex.Message);
                }
                #endregion

                #region GetAllUnitSystemId
                List<Guid> idList1 = [];
                try
                {
                    idList1 = (List<Guid>)await nSwagClient.GetAllUnitSystemIdAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get all UnitSystem ids\n" + ex.Message);
                }
                Assert.That(idList1, Is.Not.Null);
                Assert.That(idList1, Does.Contain(guid));
                #endregion

                #region GetAllUnitSystemById
                UnitSystem? unitSystem2 = null;
                try
                {
                    unitSystem2 = await nSwagClient.GetUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get UnitSystem of given Id\n" + ex.Message);
                }
                Assert.That(unitSystem2, Is.Not.Null);
                Assert.That(unitSystem2.ID, Is.EqualTo(guid));
                #endregion

                #region GetAllUnitSystem
                List<UnitSystem> unitSystemList = [];
                try
                {
                    unitSystemList = (List<UnitSystem>)await nSwagClient.GetAllUnitSystemAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the list of UnitSystem\n" + ex.Message);
                }
                Assert.That(unitSystemList, Is.Not.Null);
                IEnumerable<UnitSystem> unitSystemList2 =
                    from elt in unitSystemList
                    where elt.ID == guid
                    select elt;
                Assert.That(unitSystemList2, Is.Not.Null);
                Assert.That(unitSystemList2, Is.Not.Empty);
                #endregion

                #region GetAllUnitSystemLight
                List<UnitSystemLight> unitSystemLightList = [];
                try
                {
                    unitSystemLightList = (List<UnitSystemLight>)await nSwagClient.GetAllUnitSystemLightAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the list of UnitSystemLight\n" + ex.Message);
                }
                Assert.That(unitSystemLightList, Is.Not.Null);
                Assert.That(unitSystemLightList, Is.Not.Empty);
                IEnumerable<UnitSystemLight> unitSystemLightList2 =
                    from elt in unitSystemLightList
                    where elt.Name == unitSystem.Name
                    select elt;
                Assert.That(unitSystemLightList2, Is.Not.Null);
                Assert.That(unitSystemLightList2, Is.Not.Empty);
                #endregion

                #region finally delete the new ID
                unitSystem2 = null;
                try
                {
                    await nSwagClient.DeleteUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitSystem of given Id\n" + ex.Message);
                }
                try
                {
                    unitSystem2 = await nSwagClient.GetUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get UnitSystem of given Id\n" + ex.Message);
                }
                Assert.That(unitSystem2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitSystem_Post()
        {
            if (!bypassTests)
            {
                #region trying to post an empty guid
                Guid guid = Guid.Empty;
                UnitSystem unitSystem = new()
                {
                    ID = guid,
                    Name = "My test UnitSystem for Post",
                    Description = "Test",
                    IsDefault = false,
                    IsSI = false,
                    Choices = new Dictionary<string, string>()
                };
                UnitSystem? unitSystem2 = null;
                try
                {
                    await nSwagClient.PostUnitSystemAsync(unitSystem);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(400));
                    TestContext.WriteLine("Impossible to Post UnitSystem with empty Guid\n" + ex.Message);
                }
                try
                {
                    unitSystem2 = await nSwagClient.GetUnitSystemByIdAsync(Guid.Empty);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(400));
                    TestContext.WriteLine("Impossible to Get UnitSystem identified by an empty Guid\n" + ex.Message);
                }
                Assert.That(unitSystem2, Is.Null);
                #endregion

                #region post some corrupted data
                // post data with missing input that fails the calculation process
                #endregion

                #region posting a new ID in a valid state
                guid = Guid.NewGuid();
                unitSystem = new()
                {
                    ID = guid,
                    Name = "My test UnitSystem for Post",
                    Description = "Test",
                    IsDefault = false,
                    IsSI = false,
                    Choices = new Dictionary<string, string>()
                };
                try
                {
                    await nSwagClient.PostUnitSystemAsync(unitSystem);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post UnitSystem although it is in a valid state\n" + ex.Message);
                }
                try
                {
                    unitSystem2 = await nSwagClient.GetUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the UnitSystem of given Id\n" + ex.Message);
                }
                Assert.That(unitSystem2, Is.Not.Null);
                Assert.That(unitSystem2.ID, Is.EqualTo(guid));
                Assert.That(unitSystem2.Name, Is.EqualTo(unitSystem.Name));
                #endregion

                #region trying to repost the same ID
                unitSystem = new()
                {
                    ID = guid,
                    Name = "My test UnitSystem for Post with modified name",
                    Description = "Test",
                    IsDefault = false,
                    IsSI = false,
                    Choices = new Dictionary<string, string>()
                };
                bool conflict = false;
                try
                {
                    await nSwagClient.PostUnitSystemAsync(unitSystem);
                }
                catch (ApiException ex)
                {
                    conflict = true;
                    Assert.That(ex.StatusCode, Is.EqualTo(409));
                    TestContext.WriteLine("Impossible to Post existing UnitSystem\n" + ex.Message);
                }
                Assert.That(conflict, Is.True);
                #endregion

                #region finally delete the new ID
                unitSystem2 = null;
                try
                {
                    await nSwagClient.DeleteUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitSystem of given Id\n" + ex.Message);
                }
                try
                {
                    unitSystem2 = await nSwagClient.GetUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get deleted UnitSystem of given Id\n" + ex.Message);
                }
                Assert.That(unitSystem2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitSystem_Put()
        {
            if (!bypassTests)
            {
                #region posting a new ID
                Guid guid = Guid.NewGuid();
                UnitSystem unitSystem = new()
                {
                    ID = guid,
                    Name = "My test UnitSystem for Put",
                    Description = "Test",
                    IsDefault = false,
                    IsSI = false,
                    Choices = new Dictionary<string, string>()
                };
                UnitSystem? unitSystem2 = null;
                try
                {
                    await nSwagClient.PostUnitSystemAsync(unitSystem);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post UnitSystem\n" + ex.Message);
                }
                try
                {
                    unitSystem2 = await nSwagClient.GetUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the UnitSystem of given Id\n" + ex.Message);
                }
                Assert.That(unitSystem2, Is.Not.Null);
                Assert.That(unitSystem2.ID, Is.EqualTo(guid));
                Assert.That(unitSystem2.Name, Is.EqualTo(unitSystem.Name));
                #endregion

                #region updating the new Id
                unitSystem = new()
                {
                    ID = guid,
                    Name = "My test UnitSystem for Put with modified name",
                    Description = "Test",
                    IsDefault = false,
                    IsSI = false,
                    Choices = new Dictionary<string, string>()
                };
                try
                {
                    await nSwagClient.PutUnitSystemByIdAsync(unitSystem.ID, unitSystem);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Put UnitSystem of given Id\n" + ex.Message);
                }
                try
                {
                    unitSystem2 = await nSwagClient.GetUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the updated UnitSystem of given Id\n" + ex.Message);
                }
                Assert.That(unitSystem2, Is.Not.Null);
                Assert.That(unitSystem2.ID, Is.EqualTo(guid));
                Assert.That(unitSystem2.Name, Is.EqualTo(unitSystem.Name));
                #endregion

                #region finally delete the new ID
                unitSystem2 = null;
                try
                {
                    await nSwagClient.DeleteUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitSystem of given Id\n" + ex.Message);
                }
                try
                {
                    unitSystem2 = await nSwagClient.GetUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get deleted UnitSystem of given Id\n" + ex.Message);
                }
                Assert.That(unitSystem2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_UnitSystem_Delete()
        {
            if (!bypassTests)
            {
                #region posting a new ID
                Guid guid = Guid.NewGuid();
                UnitSystem unitSystem = new()
                {
                    ID = guid,
                    Name = "My test UnitSystem for delete",
                    Description = "Test",
                    IsDefault = false,
                    IsSI = false,
                    Choices = new Dictionary<string, string>()
                };
                UnitSystem? unitSystem2 = null;
                try
                {
                    await nSwagClient.PostUnitSystemAsync(unitSystem);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Post UnitSystem\n" + ex.Message);
                }
                try
                {
                    unitSystem2 = await nSwagClient.GetUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get the UnitSystem of given Id\n" + ex.Message);
                }
                Assert.That(unitSystem2, Is.Not.Null);
                Assert.That(unitSystem2.ID, Is.EqualTo(guid));
                #endregion

                #region finally delete the new ID
                unitSystem2 = null;
                try
                {
                    await nSwagClient.DeleteUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Delete UnitSystem of given Id\n" + ex.Message);
                }
                try
                {
                    unitSystem2 = await nSwagClient.GetUnitSystemByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                    TestContext.WriteLine("Impossible to Get deleted UnitSystem of given Id\n" + ex.Message);
                }
                Assert.That(unitSystem2, Is.Null);
                #endregion
            }
        }

        [Test]
        public async Task Test_PhysicalQuantity_Get()
        {
            if (!bypassTests)
            {
                #region GetAllPhysicalQuantityId
                List<Guid> idList = [];
                try
                {
                    idList = (List<Guid>)await nSwagClient.GetAllPhysicalQuantityIdAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get all PhysicalQuantity ids\n" + ex.Message);
                }
                Assert.That(idList, Is.Not.Null);
                Assert.That(idList, Has.Count.EqualTo(153));
                Assert.That(idList, Does.Contain(new Guid("454a7b6b-a921-428e-8aa7-a4a636a58e34")));
                Assert.That(idList, Does.Contain(new Guid("200be1eb-c278-447c-9b15-32d20fc778b9")));
                Assert.That(idList, Does.Contain(new Guid("03bb57e6-ca8b-4741-a211-9cf57c8fd177")));
                Assert.That(idList, Does.Contain(new Guid("aed9c464-1073-448b-be62-a6a0c2a53dbc")));
                Assert.That(idList, Does.Contain(new Guid("688ccd2b-6a30-4ccc-8580-a80c3a5803fa")));
                Assert.That(idList, Does.Contain(new Guid("2a892bab-1b39-4ae4-b2d2-989621b09557")));
                Assert.That(idList, Does.Contain(new Guid("1e7af8b8-0267-4d5d-a162-59123a8fde14")));
                Assert.That(idList, Does.Contain(new Guid("bbfe7349-8cf5-4ca0-8a84-ffe66d7f33d0")));
                Assert.That(idList, Does.Contain(new Guid("037e0326-5095-4c82-ba1b-4df594243cda")));
                Assert.That(idList, Does.Contain(new Guid("2d788f1e-db66-49c3-8eb6-313152cd8e3c")));
                Assert.That(idList, Does.Contain(new Guid("5754358c-9359-4bb0-8eb4-08602d19c6af")));
                Assert.That(idList, Does.Contain(new Guid("be272506-8c7a-4eff-9a05-ad4d07f36e11")));
                Assert.That(idList, Does.Contain(new Guid("790ae2cd-170c-4908-b2b9-163ba95f5b43")));
                Assert.That(idList, Does.Contain(new Guid("c830517f-5915-4a8f-ba83-bd102c0a935f")));
                Assert.That(idList, Does.Contain(new Guid("ed95aca5-aaf9-4822-b045-342ffcd06ca7")));
                Assert.That(idList, Does.Contain(new Guid("9b284ff7-57bb-4ee0-bdbc-5fb7b80f3ae3")));
                Assert.That(idList, Does.Contain(new Guid("a322deae-e965-41bf-b4fe-a7530d33c9a0")));
                Assert.That(idList, Does.Contain(new Guid("da5094a4-7481-4246-9def-1bd3b6f893a1")));
                Assert.That(idList, Does.Contain(new Guid("3c6176f8-8f74-4fbf-bb65-207ed8b0a120")));
                Assert.That(idList, Does.Contain(new Guid("3be49c73-d2d1-40a2-b15f-07a1606d8179")));
                Assert.That(idList, Does.Contain(new Guid("a3f230b0-a70b-40dd-9305-39e63bb1821b")));
                Assert.That(idList, Does.Contain(new Guid("96058475-80c4-4394-b21a-afd2fb1594c8")));
                Assert.That(idList, Does.Contain(new Guid("fd02d171-cd96-4a41-84cc-431b50ba879b")));
                Assert.That(idList, Does.Contain(new Guid("99d13248-c303-4b3d-b062-af98de701d6f")));
                Assert.That(idList, Does.Contain(new Guid("751a8f44-d938-4319-a422-a753962fd91f")));
                Assert.That(idList, Does.Contain(new Guid("58dadec7-7858-414b-8d7b-66504d5c2793")));
                Assert.That(idList, Does.Contain(new Guid("3bb73c6f-c40e-4e54-b59a-962bec9aafed")));
                Assert.That(idList, Does.Contain(new Guid("26a7767a-ea4d-417e-a1ef-b7fe674dcd3f")));
                Assert.That(idList, Does.Contain(new Guid("16130f2d-72a8-44a5-beaa-adbb5a1a7b21")));
                Assert.That(idList, Does.Contain(new Guid("7106f7cb-ddf2-4e2f-9e21-b19bc83eb248")));
                Assert.That(idList, Does.Contain(new Guid("0f282508-9223-489d-86e6-36307f987045")));
                Assert.That(idList, Does.Contain(new Guid("9c4eb2bc-413f-456e-ae6b-b1055be8e839")));
                Assert.That(idList, Does.Contain(new Guid("7f4f645c-e23e-41bc-bbcc-1dbcef53318e")));
                Assert.That(idList, Does.Contain(new Guid("b3fd17fe-ce71-4ef3-ac99-cf4f5756e81a")));
                Assert.That(idList, Does.Contain(new Guid("e5212340-1147-4cad-9f71-5cd9d4208ffd")));
                Assert.That(idList, Does.Contain(new Guid("af9fd237-14d8-4b75-8d0b-34ea8961c20b")));
                Assert.That(idList, Does.Contain(new Guid("d9ca8230-a07a-45c0-ba67-051b70607c40")));
                Assert.That(idList, Does.Contain(new Guid("8a1ff3d9-95c9-43e1-abb4-4ae9b8df861e")));
                Assert.That(idList, Does.Contain(new Guid("f6f7ab6f-3003-49d2-a17d-92a0f81938f2")));
                Assert.That(idList, Does.Contain(new Guid("10d2d588-19b8-4822-9240-e1d278d99e32")));
                Assert.That(idList, Does.Contain(new Guid("08c247bc-a55b-460e-a9a7-150faf10bdff")));
                Assert.That(idList, Does.Contain(new Guid("55682046-ff04-4a77-9311-a9f738f790b6")));
                Assert.That(idList, Does.Contain(new Guid("04df2b82-aff0-485a-855e-3d2aa53e12eb")));
                Assert.That(idList, Does.Contain(new Guid("6c2da24b-fa92-415d-9161-150de87dad4c")));
                Assert.That(idList, Does.Contain(new Guid("69151432-d2ed-4473-a3dc-334f8e6daaa6")));
                Assert.That(idList, Does.Contain(new Guid("f8ab1afa-7b99-403b-9410-93598bbb4089")));
                Assert.That(idList, Does.Contain(new Guid("b9a3f96b-8861-4b03-9c8a-3c0d7d6ec139")));
                Assert.That(idList, Does.Contain(new Guid("3dd05c4c-3d6d-49ae-a878-5a5e4a6e7acf")));
                Assert.That(idList, Does.Contain(new Guid("413da2c1-ebad-454a-ae14-1a8620f8f59c")));
                Assert.That(idList, Does.Contain(new Guid("62eb6afe-bd7d-48dd-b4fd-de40e9f3c632")));
                Assert.That(idList, Does.Contain(new Guid("6417f6e0-969d-43f2-bee6-249199ec1697")));
                Assert.That(idList, Does.Contain(new Guid("c6c87a27-c04d-4658-8a71-1e46eb3bfd80")));
                Assert.That(idList, Does.Contain(new Guid("e9d5bfe9-428b-4df0-9fe5-d9ad17e6a0cb")));
                Assert.That(idList, Does.Contain(new Guid("ed24a9f7-b70d-4f39-a992-241f25e1a77e")));
                Assert.That(idList, Does.Contain(new Guid("e5c75fa9-0102-42dc-bb0c-830fe9fca2b9")));
                Assert.That(idList, Does.Contain(new Guid("3a317540-3db4-47a1-a566-33b6f39b7540")));
                Assert.That(idList, Does.Contain(new Guid("4c1819d5-008b-4613-b62a-3f5d91b08ee7")));
                Assert.That(idList, Does.Contain(new Guid("7c4e127d-aa65-4796-a962-c2c666c4fdd0")));
                Assert.That(idList, Does.Contain(new Guid("ca23212e-8f2d-4041-89f6-ac8bfa8604fa")));
                Assert.That(idList, Does.Contain(new Guid("b8694592-8f8d-4684-b0ba-c88de50c8486")));
                Assert.That(idList, Does.Contain(new Guid("5e509f43-8fb4-490e-b9a5-59d7393761c0")));
                Assert.That(idList, Does.Contain(new Guid("3eb9e01e-48fa-430e-82c6-3aee4d359ac4")));
                Assert.That(idList, Does.Contain(new Guid("7ffbcc35-b46f-4656-baf5-c92be501f0ec")));
                Assert.That(idList, Does.Contain(new Guid("97555d61-9fc3-4769-9143-6bc2bf51b2d7")));
                Assert.That(idList, Does.Contain(new Guid("5d356437-ab4e-4de7-8219-1f4988315dee")));
                Assert.That(idList, Does.Contain(new Guid("3716ad37-2b0c-4c0b-8936-6c9cdb47ad1d")));
                Assert.That(idList, Does.Contain(new Guid("d3aa72c5-2fc0-4024-902e-6775d63f3231")));
                Assert.That(idList, Does.Contain(new Guid("b8f8f4f5-1925-4eaf-87c2-2adfdf618454")));
                Assert.That(idList, Does.Contain(new Guid("05571702-00e6-47d7-8590-fd3983645406")));
                Assert.That(idList, Does.Contain(new Guid("d07d00aa-35aa-41c6-a52d-ad51c3f4e97f")));
                Assert.That(idList, Does.Contain(new Guid("adf7b170-8d24-4c9f-93e1-40179f361d8c")));
                Assert.That(idList, Does.Contain(new Guid("b7ab1664-3d56-4ae5-842a-e4d6d0475ef9")));
                Assert.That(idList, Does.Contain(new Guid("0d36345b-624d-47c1-9d20-e627a6c6c13a")));
                Assert.That(idList, Does.Contain(new Guid("e3d17133-1c98-4ef2-8b1b-f0d935a4c1e4")));
                Assert.That(idList, Does.Contain(new Guid("875392e2-ef43-45f7-a19b-19c51eaba248")));
                Assert.That(idList, Does.Contain(new Guid("3709c98d-d471-41dd-bfde-81c4458757e5")));
                Assert.That(idList, Does.Contain(new Guid("2f6516a1-47cc-498f-8271-e84150183665")));
                Assert.That(idList, Does.Contain(new Guid("6fd69f30-a219-4d56-a1dd-000d8175e2ed")));
                Assert.That(idList, Does.Contain(new Guid("b6c99136-8e57-4eea-9a31-fb804bc8ae4b")));
                Assert.That(idList, Does.Contain(new Guid("e6f22876-ca88-4d0e-a4a5-c76b0db3556f")));
                Assert.That(idList, Does.Contain(new Guid("046cb449-8cab-4d0c-bb28-3e2060f292e5")));
                Assert.That(idList, Does.Contain(new Guid("21fc0373-6eda-460b-bacb-070abf2f3add")));
                Assert.That(idList, Does.Contain(new Guid("e278ace8-d577-4fb4-8d1d-dd8a3d072027")));
                Assert.That(idList, Does.Contain(new Guid("82a2b5fc-9edd-45ea-80cb-1cd46d516fdb")));
                Assert.That(idList, Does.Contain(new Guid("6b3db5b2-7e30-47f7-91c5-5951dd3f9246")));
                Assert.That(idList, Does.Contain(new Guid("72228ff1-9ba8-44f0-b9b1-85fa8dfb32ea")));
                Assert.That(idList, Does.Contain(new Guid("bfec67e2-839f-47d7-968c-69287567835d")));
                Assert.That(idList, Does.Contain(new Guid("0e41ce3a-a0e4-44a3-bf6e-6c2a70f4a28b")));
                Assert.That(idList, Does.Contain(new Guid("60f2af98-56e4-4f9c-8438-59646a35fc0d")));
                Assert.That(idList, Does.Contain(new Guid("787c3f65-b6d5-4866-885b-12571b1d9734")));
                Assert.That(idList, Does.Contain(new Guid("b5ccd49c-a9fd-4cfd-8dd1-fb4d3f8c3ad5")));
                Assert.That(idList, Does.Contain(new Guid("af63f164-0fb7-42c0-ac55-06e40b6c12e5")));
                Assert.That(idList, Does.Contain(new Guid("c0d965b2-a153-420a-9d03-7a2a272d619e")));
                Assert.That(idList, Does.Contain(new Guid("fd58fca3-6221-4e85-a7aa-a021ee04e8a8")));
                Assert.That(idList, Does.Contain(new Guid("3a58147b-88db-4474-8390-dd0e0f7d206b")));
                Assert.That(idList, Does.Contain(new Guid("22443197-6bcf-45f7-9079-4f710585af60")));
                Assert.That(idList, Does.Contain(new Guid("e9b32538-f7f4-4f99-a206-0601a4e4a5f8")));
                Assert.That(idList, Does.Contain(new Guid("4410a514-a65a-48ca-82d1-ab788b3d2df9")));
                Assert.That(idList, Does.Contain(new Guid("dac9cee1-59a3-42d5-98c6-0c7baf5083bb")));
                Assert.That(idList, Does.Contain(new Guid("78942f39-d764-42f1-b270-47a3b35e5112")));
                Assert.That(idList, Does.Contain(new Guid("30c08b42-6a89-4d99-879b-882eb7ed46d0")));
                Assert.That(idList, Does.Contain(new Guid("c99547c5-b545-4060-bd82-eadc13772493")));
                Assert.That(idList, Does.Contain(new Guid("6cc821d6-b979-4bf9-b1cc-ac266b221330")));
                Assert.That(idList, Does.Contain(new Guid("1bf5cf90-84c4-4dcc-ac74-92223d3c3c46")));
                Assert.That(idList, Does.Contain(new Guid("dc474098-a2b5-44fb-b56a-0ae20ff62ad6")));
                Assert.That(idList, Does.Contain(new Guid("f4c0a6fd-5000-4507-8612-ae4374c0cacc")));
                Assert.That(idList, Does.Contain(new Guid("0e218b8e-bc7c-4902-b88d-1cdab4a5dc94")));
                Assert.That(idList, Does.Contain(new Guid("94ad3e73-2a44-4c60-bbca-188b941f3357")));
                Assert.That(idList, Does.Contain(new Guid("d7f0d3a8-2d15-4ae9-897a-5d1ef7feef8a")));
                Assert.That(idList, Does.Contain(new Guid("92a284e7-9898-41f7-950d-4ba9f1ec550b")));
                Assert.That(idList, Does.Contain(new Guid("fd799f5c-0963-4201-aec3-e531df6b226e")));
                Assert.That(idList, Does.Contain(new Guid("d9db6bb4-77af-4fc3-a683-7bedd781fcba")));
                Assert.That(idList, Does.Contain(new Guid("8817dc80-eb46-42d5-b85f-703fa8845f32")));
                Assert.That(idList, Does.Contain(new Guid("4950170a-7882-4673-9d27-3402dbbca2bb")));
                Assert.That(idList, Does.Contain(new Guid("05c59293-4e3b-4fc0-b579-12c241109610")));
                Assert.That(idList, Does.Contain(new Guid("5f180166-bc44-4855-916f-236a5a31893d")));
                Assert.That(idList, Does.Contain(new Guid("1e9bafaa-bbf1-4a29-9811-39b5e2280499")));
                Assert.That(idList, Does.Contain(new Guid("a3ad9fd6-a516-42c8-98a5-14e178dc62f2")));
                Assert.That(idList, Does.Contain(new Guid("76dd6ac8-8b67-416c-b41f-07bbf4065cdb")));
                Assert.That(idList, Does.Contain(new Guid("c198aa3b-3b24-402d-b60b-f54ff9430f33")));
                Assert.That(idList, Does.Contain(new Guid("82b91f3f-d1ec-476b-98e0-eedbba6281ec")));
                Assert.That(idList, Does.Contain(new Guid("84ce5a77-fd76-4014-ad8e-03f194c3e329")));
                Assert.That(idList, Does.Contain(new Guid("fe8fd6fd-814c-44c9-9462-f034dd46dc85")));
                Assert.That(idList, Does.Contain(new Guid("186eef6a-9da3-4b97-a6d0-d496bdf59321")));
                Assert.That(idList, Does.Contain(new Guid("559ae484-42ed-4379-86f5-67dae451a9c9")));
                Assert.That(idList, Does.Contain(new Guid("eff33c0e-1e92-49e4-a7ab-716d9d66a157")));
                Assert.That(idList, Does.Contain(new Guid("b8c9f810-d576-4523-a26f-921305c7f7b4")));
                Assert.That(idList, Does.Contain(new Guid("244ade8c-591d-44d4-bca6-3798046d90a1")));
                Assert.That(idList, Does.Contain(new Guid("7b12c115-3c20-4d45-b4cf-86af29255b14")));
                Assert.That(idList, Does.Contain(new Guid("ed01d6da-225d-4bcc-beac-55ccdb6fb0b9")));
                Assert.That(idList, Does.Contain(new Guid("55a8119f-4609-4d51-91b4-e2281c46c779")));
                Assert.That(idList, Does.Contain(new Guid("15280017-0ac6-4dad-a6e1-1efdb05e64c5")));
                Assert.That(idList, Does.Contain(new Guid("c81adbce-b90b-4889-8b79-921d95b35179")));
                Assert.That(idList, Does.Contain(new Guid("453bbddf-4979-4557-ba76-3fd3420fd97e")));
                Assert.That(idList, Does.Contain(new Guid("8ec20e78-3307-4363-9fc1-97c64a4b6e6e")));
                Assert.That(idList, Does.Contain(new Guid("126be5e9-ed09-459e-92ce-32a5fcd81f0a")));
                Assert.That(idList, Does.Contain(new Guid("1fbe2318-851d-4fd7-b711-9c23ffe544c7")));
                Assert.That(idList, Does.Contain(new Guid("28a2fe65-db50-46ec-8b1d-2a26286a5813")));
                Assert.That(idList, Does.Contain(new Guid("781affbc-fae3-40a0-a110-fd3bdfd7d41f")));
                Assert.That(idList, Does.Contain(new Guid("c88cc254-b870-44a6-b896-5863472bdcd0")));
                Assert.That(idList, Does.Contain(new Guid("20f58500-7e00-41e7-acc4-99e9de9bfd07")));
                Assert.That(idList, Does.Contain(new Guid("c2581b41-944c-410b-9805-62c4b54de510")));
                Assert.That(idList, Does.Contain(new Guid("5e75da44-a675-4f0e-a0fb-52b2cb6797ce")));
                Assert.That(idList, Does.Contain(new Guid("0076d96f-bfc3-4f98-8541-4fd12e4bcbff")));
                #endregion

                #region GetAllPhysicalQuantityById

                #region Acceleration quantity
                Guid guid = new("454a7b6b-a921-428e-8aa7-a4a636a58e34");
                PhysicalQuantity? physicalQuantity = null;
                try
                {
                    physicalQuantity = await nSwagClient.GetPhysicalQuantityByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get PhysicalQuantity of given Id\n" + ex.Message);
                }
                Assert.That(physicalQuantity, Is.Not.Null);
                Assert.Multiple(() =>
                    {
                        Assert.That(physicalQuantity.ID, Is.EqualTo(guid));
                        Assert.That(physicalQuantity.Name, Is.EqualTo("Acceleration"));
                        Assert.That(physicalQuantity.UnitChoices, Has.Count.EqualTo(20));
                    });
                #endregion

                #region Amount Substance quantity
                guid = new("200be1eb-c278-447c-9b15-32d20fc778b9");
                physicalQuantity = null;
                try
                {
                    physicalQuantity = await nSwagClient.GetPhysicalQuantityByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get PhysicalQuantity of given Id\n" + ex.Message);
                }
                Assert.That(physicalQuantity, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(physicalQuantity.ID, Is.EqualTo(guid));
                    Assert.That(physicalQuantity.Name, Is.EqualTo("AmountSubstance"));
                    Assert.That(physicalQuantity.UnitChoices, Has.Count.EqualTo(8));
                });
                #endregion

                #region AngleMagneticFluxDensity quantity
                guid = new("03bb57e6-ca8b-4741-a211-9cf57c8fd177");
                physicalQuantity = null;
                try
                {
                    physicalQuantity = await nSwagClient.GetPhysicalQuantityByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get PhysicalQuantity of given Id\n" + ex.Message);
                }
                Assert.That(physicalQuantity, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(physicalQuantity.ID, Is.EqualTo(guid));
                    Assert.That(physicalQuantity.Name, Is.EqualTo("AngleMagneticFluxDensity"));
                    Assert.That(physicalQuantity.UnitChoices, Has.Count.EqualTo(16));
                });
                #endregion

                #region AngleVariationGradient quantity
                guid = new("aed9c464-1073-448b-be62-a6a0c2a53dbc");
                physicalQuantity = null;
                try
                {
                    physicalQuantity = await nSwagClient.GetPhysicalQuantityByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get PhysicalQuantity of given Id\n" + ex.Message);
                }
                Assert.That(physicalQuantity, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(physicalQuantity.ID, Is.EqualTo(guid));
                    Assert.That(physicalQuantity.Name, Is.EqualTo("AngleGradientPerLength"));
                    Assert.That(physicalQuantity.UnitChoices, Has.Count.EqualTo(12));
                });
                #endregion

                #region AngularVelocity quantity
                guid = new("688ccd2b-6a30-4ccc-8580-a80c3a5803fa");
                physicalQuantity = null;
                try
                {
                    physicalQuantity = await nSwagClient.GetPhysicalQuantityByIdAsync(guid);
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get PhysicalQuantity of given Id\n" + ex.Message);
                }
                Assert.That(physicalQuantity, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(physicalQuantity.ID, Is.EqualTo(guid));
                    Assert.That(physicalQuantity.Name, Is.EqualTo("AngularVelocity"));
                    Assert.That(physicalQuantity.UnitChoices, Has.Count.EqualTo(8));
                });
                #endregion
                #endregion

                #region GetAllPhysicalQuantity
                List<PhysicalQuantity> physicalQuantityList = [];
                try
                {
                    physicalQuantityList = (List<PhysicalQuantity>)await nSwagClient.GetAllPhysicalQuantityAsync();
                }
                catch (ApiException ex)
                {
                    TestContext.WriteLine("Impossible to Get all PhysicalQuantity\n" + ex.Message);
                }
                Assert.That(physicalQuantityList, Is.Not.Null);
                Assert.That(physicalQuantityList, Has.Count.EqualTo(153));
                Assert.Multiple(() =>
                {
                    foreach (var qty in physicalQuantityList)
                    {
                        Assert.That(idList, Does.Contain(qty.ID));
                    }
                });
                #endregion
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            httpClient?.Dispose();
        }
    }
}
