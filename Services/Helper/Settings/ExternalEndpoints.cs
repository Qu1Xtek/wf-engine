namespace WorkflowConfigurator.Services.Helper.Settings
{
    public sealed class ExternalEndpoints
    {
        private const string _login = "LoginURL";
        private const string _base = "BaseURL";
        private const string _validate = "ValidateURL";
        private const string _create = "CreateURL";        
        private const string _prodCategories = "ProductCategories";
        private const string _checkProd = "CheckProduct";

        public string BaseURL => _base;
        public string CreateURL => _create;
        public string LoginURL => _login;
        public string ValidateURL => _validate;

        // IMS Section
        public string GetCategories => _prodCategories;

        public string CheckProduct => _checkProd;
    }
}
