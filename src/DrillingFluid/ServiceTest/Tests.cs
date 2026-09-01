using System.Net.Http.Headers;
using NORCE.Drilling.DrillingFluid.ModelShared;

namespace ServiceTest
{
    public class Tests
    {
        // testing outside Visual Studio requires using http port (https faces authentication issues both in console and on github)
        private static string host = "http://localhost:8080/";
        //private static string host = "https://localhost:5001/";
        //private static string host = "https://localhost:44368/";
        //private static string host = "http://localhost:54949/";
        private static HttpClient httpClient;
        private static Client nSwagClient;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }; // temporary workaround for testing purposes: bypass certificate validation (not recommended for production environments due to security risks)
            httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri(host + "DrillingFluid/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            nSwagClient = new Client(httpClient.BaseAddress.ToString(), httpClient);
        }

        [Test]
        public async Task Test_DrillingFluid_GET()
        {
            #region post a DrillingFluid
            // Create instance of DrillingFluid
            DrillingFluid drillingFluid = PseudoConstructors.ConstructDrillingFluid();

            //Extract metainfo
            MetaInfo metaInfo = drillingFluid.MetaInfo;
            Guid guid = metaInfo.ID;
            try
            {
                await nSwagClient.PostDrillingFluidAsync(drillingFluid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST given DrillingFluid\n" + ex.Message);
            }
            #endregion

            #region GetAllDrillingFluidId
            List<Guid> idList = [];
            try
            {
                idList = (List<Guid>)await nSwagClient.GetAllDrillingFluidIdAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET all DrillingFluid ids\n" + ex.Message);
            }
            Assert.That(idList, Is.Not.Null);
            Assert.That(idList, Does.Contain(guid));
            #endregion

            #region GetAllDrillingFluidMetaInfo
            List<MetaInfo> metaInfoList = [];
            try
            {
                metaInfoList = (List<MetaInfo>)await nSwagClient.GetAllDrillingFluidMetaInfoAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET all DrillingFluid metainfos\n" + ex.Message);
            }
            Assert.That(metaInfoList, Is.Not.Null);
            IEnumerable<MetaInfo> metaInfoList2 =
                from elt in metaInfoList
                where elt.ID == guid
                select elt;
            Assert.That(metaInfoList2, Is.Not.Null);
            Assert.That(metaInfoList2, Is.Not.Empty);
            #endregion

            #region GetAllDrillingFluidById
            DrillingFluid? drillingFluid2 = null;
            try
            {
                drillingFluid2 = await nSwagClient.GetDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET DrillingFluid of given Id\n" + ex.Message);
            }
            Assert.That(drillingFluid2, Is.Not.Null);
            Assert.That(drillingFluid2.Name, Is.EqualTo(drillingFluid.Name));
            #endregion

            #region GetAllDrillingFluidLight
            List<DrillingFluidLight> drillingFluidLightList = [];
            try
            {
                drillingFluidLightList = (List<DrillingFluidLight>)await nSwagClient.GetAllDrillingFluidLightAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the list of DrillingFluidLight\n" + ex.Message);
            }
            Assert.That(drillingFluidLightList, Is.Not.Null);
            Assert.That(drillingFluidLightList, Is.Not.Empty);
            IEnumerable<DrillingFluidLight> drillingFluidLightList2 =
                from elt in drillingFluidLightList
                where elt.Name == drillingFluid.Name
                select elt;
            Assert.That(drillingFluidLightList2, Is.Not.Null);
            Assert.That(drillingFluidLightList2, Is.Not.Empty);
            #endregion

            #region GetAllDrillingFluid
            List<DrillingFluid> drillingFluidList = new();
            try
            {
                drillingFluidList = (List<DrillingFluid>)await nSwagClient.GetAllDrillingFluidAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the list of DrillingFluid\n" + ex.Message);
            }
            Assert.That(drillingFluidList, Is.Not.Null);
            IEnumerable<DrillingFluid> drillingFluidList2 =
                from elt in drillingFluidList
                where elt.Name == drillingFluid.Name
                select elt;
            Assert.That(drillingFluidList2, Is.Not.Null);
            Assert.That(drillingFluidList2, Is.Not.Empty);
            #endregion

            #region finally delete the new ID
            drillingFluid2 = null;
            try
            {
                await nSwagClient.DeleteDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE DrillingFluid of given Id\n" + ex.Message);
            }
            try
            {
                drillingFluid2 = await nSwagClient.GetDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET DrillingFluid of given Id\n" + ex.Message);
            }
            Assert.That(drillingFluid2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_DrillingFluid_POST()
        {
            #region trying to post an empty guid
            // Create instance of drillingFluid
            DrillingFluid drillingFluid = PseudoConstructors.ConstructDrillingFluid();
            drillingFluid.MetaInfo.ID = Guid.Empty;
            //Extract metainfo
            MetaInfo metaInfo = drillingFluid.MetaInfo;
            DrillingFluid? drillingFluid2 = null;
            try
            {
                await nSwagClient.PostDrillingFluidAsync(drillingFluid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(400));
                TestContext.WriteLine("Impossible to POST DrillingFluid with empty Guid\n" + ex.Message);
            }
            try
            {
                drillingFluid2 = await nSwagClient.GetDrillingFluidByIdAsync(Guid.Empty);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(400));
                TestContext.WriteLine("Impossible to GET DrillingFluid identified by an empty Guid\n" + ex.Message);
            }
            Assert.That(drillingFluid2, Is.Null);
            #endregion

            #region post some corrupted data
            // post data with missing input that fails the calculation process
            #endregion

            #region posting a new ID in a valid state
            Guid guid = Guid.NewGuid();
            metaInfo = new() { ID = guid };
            drillingFluid.MetaInfo = metaInfo;
            try
            {
                await nSwagClient.PostDrillingFluidAsync(drillingFluid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST DrillingFluid although it is in a valid state\n" + ex.Message);
            }
            try
            {
                drillingFluid2 = await nSwagClient.GetDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the DrillingFluid of given Id\n" + ex.Message);
            }
            Assert.That(drillingFluid2, Is.Not.Null);
            Assert.That(drillingFluid2.MetaInfo, Is.Not.Null);
            Assert.That(drillingFluid2.MetaInfo.ID, Is.EqualTo(guid));
            Assert.That(drillingFluid2.Name, Is.EqualTo(drillingFluid.Name));
            #endregion

            #region trying to repost the same ID
            bool conflict = false;
            try
            {
                await nSwagClient.PostDrillingFluidAsync(drillingFluid);
            }
            catch (ApiException ex)
            {
                conflict = true;
                Assert.That(ex.StatusCode, Is.EqualTo(409));
                TestContext.WriteLine("Impossible to POST existing DrillingFluid\n" + ex.Message);
            }
            Assert.That(conflict, Is.True);
            #endregion

            #region finally delete the new ID
            drillingFluid2 = null;
            try
            {
                await nSwagClient.DeleteDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE DrillingFluid of given Id\n" + ex.Message);
            }
            try
            {
                drillingFluid2 = await nSwagClient.GetDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted DrillingFluid of given Id\n" + ex.Message);
            }
            Assert.That(drillingFluid2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_DrillingFluid_PUT()
        {
            #region posting a new ID
            // Create instance of drillingFluid
            DrillingFluid drillingFluid = PseudoConstructors.ConstructDrillingFluid();
            //Extract metainfo
            MetaInfo metaInfo = drillingFluid.MetaInfo;
            Guid guid = metaInfo.ID;
            DrillingFluid? drillingFluid2 = null;
            try
            {
                await nSwagClient.PostDrillingFluidAsync(drillingFluid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST DrillingFluid\n" + ex.Message);
            }
            try
            {
                drillingFluid2 = await nSwagClient.GetDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the DrillingFluid of given Id\n" + ex.Message);
            }
            Assert.That(drillingFluid2, Is.Not.Null);
            Assert.That(drillingFluid2.MetaInfo, Is.Not.Null);
            Assert.That(drillingFluid2.MetaInfo.ID, Is.EqualTo(guid));
            Assert.That(drillingFluid2.Name, Is.EqualTo(drillingFluid.Name));
            #endregion

            #region updating the new Id
            drillingFluid.Name = "My test DrillingFluid with modified name";
            drillingFluid.LastModificationDate = DateTimeOffset.UtcNow;
            try
            {
                await nSwagClient.PutDrillingFluidByIdAsync(drillingFluid.MetaInfo.ID, drillingFluid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to PUT DrillingFluid of given Id\n" + ex.Message);
            }
            try
            {
                drillingFluid2 = await nSwagClient.GetDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the updated DrillingFluid of given Id\n" + ex.Message);
            }
            Assert.That(drillingFluid2, Is.Not.Null);
            Assert.That(drillingFluid2.MetaInfo, Is.Not.Null);
            Assert.That(drillingFluid2.MetaInfo.ID, Is.EqualTo(drillingFluid.MetaInfo.ID));
            Assert.That(drillingFluid2.Name, Is.EqualTo(drillingFluid.Name));
            #endregion

            #region finally delete the new ID
            drillingFluid2 = null;
            try
            {
                await nSwagClient.DeleteDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE DrillingFluid of given Id\n" + ex.Message);
            }
            try
            {
                drillingFluid2 = await nSwagClient.GetDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted DrillingFluid of given Id\n" + ex.Message);
            }
            Assert.That(drillingFluid2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_DrillingFluid_DELETE()
        {
            #region posting a new ID
            // Create instance of drillingFluid
            DrillingFluid drillingFluid = PseudoConstructors.ConstructDrillingFluid();
            //Extract metainfo
            MetaInfo metaInfo = drillingFluid.MetaInfo;
            Guid guid = metaInfo.ID;
            DrillingFluid? drillingFluid2 = null;
            try
            {
                await nSwagClient.PostDrillingFluidAsync(drillingFluid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST DrillingFluid\n" + ex.Message);
            }
            try
            {
                drillingFluid2 = await nSwagClient.GetDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the DrillingFluid of given Id\n" + ex.Message);
            }
            Assert.That(drillingFluid2, Is.Not.Null);
            Assert.That(drillingFluid2.MetaInfo, Is.Not.Null);
            Assert.That(drillingFluid2.MetaInfo.ID, Is.EqualTo(drillingFluid.MetaInfo.ID));
            Assert.That(drillingFluid2.Name, Is.EqualTo(drillingFluid.Name));
            #endregion

            #region finally delete the new ID
            drillingFluid2 = null;
            try
            {
                await nSwagClient.DeleteDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE DrillingFluid of given Id\n" + ex.Message);
            }
            try
            {
                drillingFluid2 = await nSwagClient.GetDrillingFluidByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted DrillingFluid of given Id\n" + ex.Message);
            }
            Assert.That(drillingFluid2, Is.Null);
            #endregion
        }
       
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            httpClient?.Dispose();
        }
    }
}