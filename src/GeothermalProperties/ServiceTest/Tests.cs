using System.Net.Http.Headers;
using NORCE.Drilling.GeothermalProperties.ModelShared;

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
            httpClient.BaseAddress = new Uri(host + "GeothermalProperties/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            nSwagClient = new Client(httpClient.BaseAddress.ToString(), httpClient);
        }

        [Test]
        public async Task Test_GeothermalPropertiesCompletionOrder_GET()
        {
            #region post a GeothermalPropertiesCompletionOrder
            // Create instance of GeothermalPropertiesCompletionOrder
            GeothermalPropertiesCompletionOrder geothermalPropertiesCompletionOrder = PseudoConstructors.ConstructGeothermalPropertiesCompletionOrder();

            //Extract metainfo
            MetaInfo metaInfo = geothermalPropertiesCompletionOrder.MetaInfo;
            Guid guid = metaInfo.ID;
            try
            {
                await nSwagClient.PostGeothermalPropertiesCompletionOrderAsync(geothermalPropertiesCompletionOrder);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST given GeothermalPropertiesCompletionOrder\n" + ex.Message);
            }
            #endregion

            #region GetAllGeothermalPropertiesCompletionOrderId
            List<Guid> idList = [];
            try
            {
                idList = (List<Guid>)await nSwagClient.GetAllGeothermalPropertiesCompletionOrderIdAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET all GeothermalPropertiesCompletionOrder ids\n" + ex.Message);
            }
            Assert.That(idList, Is.Not.Null);
            Assert.That(idList, Does.Contain(guid));
            #endregion

            #region GetAllGeothermalPropertiesCompletionOrderMetaInfo
            List<MetaInfo> metaInfoList = [];
            try
            {
                metaInfoList = (List<MetaInfo>)await nSwagClient.GetAllGeothermalPropertiesCompletionOrderMetaInfoAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET all GeothermalPropertiesCompletionOrder metainfos\n" + ex.Message);
            }
            Assert.That(metaInfoList, Is.Not.Null);
            IEnumerable<MetaInfo> metaInfoList2 =
                from elt in metaInfoList
                where elt.ID == guid
                select elt;
            Assert.That(metaInfoList2, Is.Not.Null);
            Assert.That(metaInfoList2, Is.Not.Empty);
            #endregion

            #region GetAllGeothermalPropertiesCompletionOrderById
            GeothermalPropertiesCompletionOrder? geothermalPropertiesCompletionOrder2 = null;
            try
            {
                geothermalPropertiesCompletionOrder2 = await nSwagClient.GetGeothermalPropertiesCompletionOrderByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET GeothermalPropertiesCompletionOrder of given Id\n" + ex.Message);
            }
            Assert.That(geothermalPropertiesCompletionOrder2, Is.Not.Null);
            Assert.That(geothermalPropertiesCompletionOrder2.Name, Is.EqualTo(geothermalPropertiesCompletionOrder.Name));
            #endregion

            #region GetAllGeothermalPropertiesCompletionOrderLight
            List<GeothermalPropertiesCompletionOrderLight> geothermalPropertiesCompletionOrderLightList = [];
            try
            {
                geothermalPropertiesCompletionOrderLightList = (List<GeothermalPropertiesCompletionOrderLight>)await nSwagClient.GetAllGeothermalPropertiesCompletionOrderLightAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the list of GeothermalPropertiesCompletionOrderLight\n" + ex.Message);
            }
            Assert.That(geothermalPropertiesCompletionOrderLightList, Is.Not.Null);
            Assert.That(geothermalPropertiesCompletionOrderLightList, Is.Not.Empty);
            IEnumerable<GeothermalPropertiesCompletionOrderLight> geothermalPropertiesCompletionOrderLightList2 =
                from elt in geothermalPropertiesCompletionOrderLightList
                where elt.Name == geothermalPropertiesCompletionOrder.Name
                select elt;
            Assert.That(geothermalPropertiesCompletionOrderLightList2, Is.Not.Null);
            Assert.That(geothermalPropertiesCompletionOrderLightList2, Is.Not.Empty);
            #endregion

            #region GetAllGeothermalPropertiesCompletionOrder
            List<GeothermalPropertiesCompletionOrder> geothermalPropertiesCompletionOrderList = new();
            try
            {
                geothermalPropertiesCompletionOrderList = (List<GeothermalPropertiesCompletionOrder>)await nSwagClient.GetAllGeothermalPropertiesCompletionOrderAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the list of GeothermalPropertiesCompletionOrder\n" + ex.Message);
            }
            Assert.That(geothermalPropertiesCompletionOrderList, Is.Not.Null);
            IEnumerable<GeothermalPropertiesCompletionOrder> geothermalPropertiesCompletionOrderList2 =
                from elt in geothermalPropertiesCompletionOrderList
                where elt.Name == geothermalPropertiesCompletionOrder.Name
                select elt;
            Assert.That(geothermalPropertiesCompletionOrderList2, Is.Not.Null);
            Assert.That(geothermalPropertiesCompletionOrderList2, Is.Not.Empty);
            #endregion

            #region finally delete the new ID
            geothermalPropertiesCompletionOrder2 = null;
            try
            {
                await nSwagClient.DeleteGeothermalPropertiesCompletionOrderByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE GeothermalPropertiesCompletionOrder of given Id\n" + ex.Message);
            }
            try
            {
                geothermalPropertiesCompletionOrder2 = await nSwagClient.GetGeothermalPropertiesCompletionOrderByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET GeothermalPropertiesCompletionOrder of given Id\n" + ex.Message);
            }
            Assert.That(geothermalPropertiesCompletionOrder2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_GeothermalPropertiesCompletionOrder_POST()
        {
            #region trying to post an empty guid
            // Create instance of geothermalPropertiesCompletionOrder
            GeothermalPropertiesCompletionOrder geothermalPropertiesCompletionOrder = PseudoConstructors.ConstructGeothermalPropertiesCompletionOrder();
            geothermalPropertiesCompletionOrder.MetaInfo.ID = Guid.Empty;
            //Extract metainfo
            MetaInfo metaInfo = geothermalPropertiesCompletionOrder.MetaInfo;
            GeothermalPropertiesCompletionOrder? geothermalPropertiesCompletionOrder2 = null;
            try
            {
                await nSwagClient.PostGeothermalPropertiesCompletionOrderAsync(geothermalPropertiesCompletionOrder);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(400));
                TestContext.WriteLine("Impossible to POST GeothermalPropertiesCompletionOrder with empty Guid\n" + ex.Message);
            }
            try
            {
                geothermalPropertiesCompletionOrder2 = await nSwagClient.GetGeothermalPropertiesCompletionOrderByIdAsync(Guid.Empty);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(400));
                TestContext.WriteLine("Impossible to GET GeothermalPropertiesCompletionOrder identified by an empty Guid\n" + ex.Message);
            }
            Assert.That(geothermalPropertiesCompletionOrder2, Is.Null);
            #endregion

            #region post some corrupted data
            // post data with missing input that fails the calculation process
            #endregion

            #region posting a new ID in a valid state
            Guid guid = Guid.NewGuid();
            metaInfo = new() { ID = guid };
            geothermalPropertiesCompletionOrder.MetaInfo = metaInfo;
            try
            {
                await nSwagClient.PostGeothermalPropertiesCompletionOrderAsync(geothermalPropertiesCompletionOrder);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST GeothermalPropertiesCompletionOrder although it is in a valid state\n" + ex.Message);
            }
            try
            {
                geothermalPropertiesCompletionOrder2 = await nSwagClient.GetGeothermalPropertiesCompletionOrderByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the GeothermalPropertiesCompletionOrder of given Id\n" + ex.Message);
            }
            Assert.That(geothermalPropertiesCompletionOrder2, Is.Not.Null);
            Assert.That(geothermalPropertiesCompletionOrder2.MetaInfo, Is.Not.Null);
            Assert.That(geothermalPropertiesCompletionOrder2.MetaInfo.ID, Is.EqualTo(guid));
            Assert.That(geothermalPropertiesCompletionOrder2.Name, Is.EqualTo(geothermalPropertiesCompletionOrder.Name));
            #endregion

            #region trying to repost the same ID
            bool conflict = false;
            try
            {
                await nSwagClient.PostGeothermalPropertiesCompletionOrderAsync(geothermalPropertiesCompletionOrder);
            }
            catch (ApiException ex)
            {
                conflict = true;
                Assert.That(ex.StatusCode, Is.EqualTo(409));
                TestContext.WriteLine("Impossible to POST existing GeothermalPropertiesCompletionOrder\n" + ex.Message);
            }
            Assert.That(conflict, Is.True);
            #endregion

            #region finally delete the new ID
            geothermalPropertiesCompletionOrder2 = null;
            try
            {
                await nSwagClient.DeleteGeothermalPropertiesCompletionOrderByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE GeothermalPropertiesCompletionOrder of given Id\n" + ex.Message);
            }
            try
            {
                geothermalPropertiesCompletionOrder2 = await nSwagClient.GetGeothermalPropertiesCompletionOrderByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted GeothermalPropertiesCompletionOrder of given Id\n" + ex.Message);
            }
            Assert.That(geothermalPropertiesCompletionOrder2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_GeothermalPropertiesCompletionOrder_DELETE()
        {
            #region posting a new ID
            // Create instance of geothermalPropertiesCompletionOrder
            GeothermalPropertiesCompletionOrder geothermalPropertiesCompletionOrder = PseudoConstructors.ConstructGeothermalPropertiesCompletionOrder();
            //Extract metainfo
            MetaInfo metaInfo = geothermalPropertiesCompletionOrder.MetaInfo;
            Guid guid = metaInfo.ID;
            GeothermalPropertiesCompletionOrder? geothermalPropertiesCompletionOrder2 = null;
            try
            {
                await nSwagClient.PostGeothermalPropertiesCompletionOrderAsync(geothermalPropertiesCompletionOrder);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST GeothermalPropertiesCompletionOrder\n" + ex.Message);
            }
            try
            {
                geothermalPropertiesCompletionOrder2 = await nSwagClient.GetGeothermalPropertiesCompletionOrderByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the GeothermalPropertiesCompletionOrder of given Id\n" + ex.Message);
            }
            Assert.That(geothermalPropertiesCompletionOrder2, Is.Not.Null);
            Assert.That(geothermalPropertiesCompletionOrder2.MetaInfo, Is.Not.Null);
            Assert.That(geothermalPropertiesCompletionOrder2.MetaInfo.ID, Is.EqualTo(geothermalPropertiesCompletionOrder.MetaInfo.ID));
            Assert.That(geothermalPropertiesCompletionOrder2.Name, Is.EqualTo(geothermalPropertiesCompletionOrder.Name));
            #endregion

            #region finally delete the new ID
            geothermalPropertiesCompletionOrder2 = null;
            try
            {
                await nSwagClient.DeleteGeothermalPropertiesCompletionOrderByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE GeothermalPropertiesCompletionOrder of given Id\n" + ex.Message);
            }
            try
            {
                geothermalPropertiesCompletionOrder2 = await nSwagClient.GetGeothermalPropertiesCompletionOrderByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted GeothermalPropertiesCompletionOrder of given Id\n" + ex.Message);
            }
            Assert.That(geothermalPropertiesCompletionOrder2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_GeothermalProperties_GET()
        {
            #region post a GeothermalProperties
            // Create instance of geothermalProperties
            GeothermalProperties geothermalProperties = PseudoConstructors.ConstructGeothermalProperties();
            MetaInfo metaInfo = geothermalProperties.MetaInfo;
            Guid guid = metaInfo.ID;

            try
            {
                await nSwagClient.PostGeothermalPropertiesAsync(geothermalProperties);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST given GeothermalProperties\n" + ex.Message);
            }
            #endregion

            #region GetAllGeothermalPropertiesId
            List<Guid?> idList = [];
            try
            {
                idList = (List<Guid?>)await nSwagClient.GetAllGeothermalPropertiesIdAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET all GeothermalProperties ids\n" + ex.Message);
            }
            Assert.That(idList, Is.Not.Null);
            Assert.That(idList, Does.Contain(guid));
            #endregion

            #region GetAllGeothermalPropertiesMetaInfo
            List<MetaInfo> metaInfoList = [];
            try
            {
                metaInfoList = (List<MetaInfo>)await nSwagClient.GetAllGeothermalPropertiesMetaInfoAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET all GeothermalProperties metainfos\n" + ex.Message);
            }
            Assert.That(metaInfoList, Is.Not.Null);
            IEnumerable<MetaInfo> metaInfoList2 =
                from elt in metaInfoList
                where elt.ID == guid
                select elt;
            Assert.That(metaInfoList2, Is.Not.Null);
            Assert.That(metaInfoList2, Is.Not.Empty);
            #endregion

            #region GetAllGeothermalPropertiesById
            GeothermalProperties? geothermalProperties2 = null;
            try
            {
                geothermalProperties2 = await nSwagClient.GetGeothermalPropertiesByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET GeothermalProperties of given Id\n" + ex.Message);
            }
            Assert.That(geothermalProperties2, Is.Not.Null);
            Assert.That(geothermalProperties2.MetaInfo.ID, Is.EqualTo(guid));
            Assert.That(geothermalProperties2.Name, Is.EqualTo(geothermalProperties.Name));
            #endregion

            #region GetAllGeothermalProperties
            List<GeothermalProperties> geothermalPropertiesList = [];
            try
            {
                geothermalPropertiesList = (List<GeothermalProperties>)await nSwagClient.GetAllGeothermalPropertiesAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the list of GeothermalProperties\n" + ex.Message);
            }
            Assert.That(geothermalPropertiesList, Is.Not.Null);
            IEnumerable<GeothermalProperties> geothermalPropertiesList2 =
                from elt in geothermalPropertiesList
                where elt.Name == geothermalProperties.Name
                select elt;
            Assert.That(geothermalPropertiesList2, Is.Not.Null);
            Assert.That(geothermalPropertiesList2, Is.Not.Empty);
            #endregion

            #region finally delete the new ID
            geothermalProperties2 = null;
            try
            {
                await nSwagClient.DeleteGeothermalPropertiesByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE GeothermalProperties of given Id\n" + ex.Message);
            }
            try
            {
                geothermalProperties2 = await nSwagClient.GetGeothermalPropertiesByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET GeothermalProperties of given Id\n" + ex.Message);
            }
            Assert.That(geothermalProperties2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_GeothermalProperties_POST()
        {
            #region trying to post an empty guid
            // Create instance of geothermalProperties
            GeothermalProperties geothermalProperties = PseudoConstructors.ConstructGeothermalProperties();
            MetaInfo metaInfo = geothermalProperties.MetaInfo;

            GeothermalProperties? geothermalProperties2 = null;
            try
            {
                await nSwagClient.PostGeothermalPropertiesAsync(geothermalProperties);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(400));
                TestContext.WriteLine("Impossible to POST GeothermalProperties with empty Guid\n" + ex.Message);
            }
            try
            {
                geothermalProperties2 = await nSwagClient.GetGeothermalPropertiesByIdAsync(Guid.Empty);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(400));
                TestContext.WriteLine("Impossible to GET GeothermalProperties identified by an empty Guid\n" + ex.Message);
            }
            Assert.That(geothermalProperties2, Is.Null);
            #endregion

            #region posting a new ID in a valid state
            Guid guid = Guid.NewGuid();
            metaInfo = new() { ID = guid };
            geothermalProperties.MetaInfo = metaInfo;
            try
            {
                await nSwagClient.PostGeothermalPropertiesAsync(geothermalProperties);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST GeothermalProperties although it is in a valid state\n" + ex.Message);
            }
            try
            {
                geothermalProperties2 = await nSwagClient.GetGeothermalPropertiesByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the GeothermalProperties of given Id\n" + ex.Message);
            }
            Assert.That(geothermalProperties2, Is.Not.Null);
            Assert.That(geothermalProperties2.MetaInfo, Is.Not.Null);
            Assert.That(geothermalProperties2.MetaInfo.ID, Is.EqualTo(guid));
            Assert.That(geothermalProperties2.Name, Is.EqualTo(geothermalProperties.Name));
            #endregion

            #region trying to repost the same ID
            bool conflict = false;
            try
            {
                await nSwagClient.PostGeothermalPropertiesAsync(geothermalProperties);
            }
            catch (ApiException ex)
            {
                conflict = true;
                Assert.That(ex.StatusCode, Is.EqualTo(409));
                TestContext.WriteLine("Impossible to POST existing GeothermalProperties\n" + ex.Message);
            }
            Assert.That(conflict, Is.True);
            #endregion

            #region finally delete the new ID
            geothermalProperties2 = null;
            try
            {
                await nSwagClient.DeleteGeothermalPropertiesByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE GeothermalProperties of given Id\n" + ex.Message);
            }
            try
            {
                geothermalProperties2 = await nSwagClient.GetGeothermalPropertiesByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted GeothermalProperties of given Id\n" + ex.Message);
            }
            Assert.That(geothermalProperties2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_GeothermalProperties_PUT()
        {
            #region posting a new ID
            // Create instance of geothermalProperties
            GeothermalProperties geothermalProperties = PseudoConstructors.ConstructGeothermalProperties();
            MetaInfo metaInfo = geothermalProperties.MetaInfo;
            Guid guid = metaInfo.ID;

            GeothermalProperties? geothermalProperties2 = null;
            try
            {
                await nSwagClient.PostGeothermalPropertiesAsync(geothermalProperties);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST GeothermalProperties\n" + ex.Message);
            }
            try
            {
                geothermalProperties2 = await nSwagClient.GetGeothermalPropertiesByIdAsync(geothermalProperties.MetaInfo.ID);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the GeothermalProperties of given Id\n" + ex.Message);
            }
            Assert.That(geothermalProperties2, Is.Not.Null);
            Assert.That(geothermalProperties2.MetaInfo, Is.Not.Null);
            Assert.That(geothermalProperties2.MetaInfo.ID, Is.EqualTo(geothermalProperties.MetaInfo.ID));
            Assert.That(geothermalProperties2.Name, Is.EqualTo(geothermalProperties.Name));
            #endregion

            #region updating the new Id
            geothermalProperties.Name = "My test GeothermalProperties with modified name";
            geothermalProperties.LastModificationDate = DateTimeOffset.UtcNow;
            try
            {
                await nSwagClient.PutGeothermalPropertiesByIdAsync(geothermalProperties.MetaInfo.ID, geothermalProperties);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to PUT GeothermalProperties of given Id\n" + ex.Message);
            }
            try
            {
                geothermalProperties2 = await nSwagClient.GetGeothermalPropertiesByIdAsync(geothermalProperties.MetaInfo.ID);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the updated GeothermalProperties of given Id\n" + ex.Message);
            }
            Assert.That(geothermalProperties2, Is.Not.Null);
            Assert.That(geothermalProperties2.MetaInfo, Is.Not.Null);
            Assert.That(geothermalProperties2.MetaInfo.ID, Is.EqualTo(geothermalProperties.MetaInfo.ID));
            Assert.That(geothermalProperties2.Name, Is.EqualTo(geothermalProperties.Name));
            #endregion

            #region finally delete the new ID
            geothermalProperties2 = null;
            try
            {
                await nSwagClient.DeleteGeothermalPropertiesByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE GeothermalProperties of given Id\n" + ex.Message);
            }
            try
            {
                geothermalProperties2 = await nSwagClient.GetGeothermalPropertiesByIdAsync(geothermalProperties.MetaInfo.ID);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted GeothermalProperties of given Id\n" + ex.Message);
            }
            Assert.That(geothermalProperties2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_GeothermalProperties_DELETE()
        {
            #region posting a new ID
            // Create instance of geothermalProperties
            GeothermalProperties geothermalProperties = PseudoConstructors.ConstructGeothermalProperties();
            MetaInfo metaInfo = geothermalProperties.MetaInfo;
            Guid guid = metaInfo.ID;

            GeothermalProperties? geothermalProperties2 = null;
            try
            {
                await nSwagClient.PostGeothermalPropertiesAsync(geothermalProperties);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST GeothermalProperties\n" + ex.Message);
            }
            try
            {
                geothermalProperties2 = await nSwagClient.GetGeothermalPropertiesByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the GeothermalProperties of given Id\n" + ex.Message);
            }
            Assert.That(geothermalProperties2, Is.Not.Null);
            Assert.That(geothermalProperties2.MetaInfo, Is.Not.Null);
            Assert.That(geothermalProperties2.MetaInfo.ID, Is.EqualTo(guid));
            Assert.That(geothermalProperties2.Name, Is.EqualTo(geothermalProperties.Name));
            #endregion

            #region finally delete the new ID
            geothermalProperties2 = null;
            try
            {
                await nSwagClient.DeleteGeothermalPropertiesByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE GeothermalProperties of given Id\n" + ex.Message);
            }
            try
            {
                geothermalProperties2 = await nSwagClient.GetGeothermalPropertiesByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted GeothermalProperties of given Id\n" + ex.Message);
            }
            Assert.That(geothermalProperties2, Is.Null);
            #endregion
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            httpClient?.Dispose();
        }
    }
}