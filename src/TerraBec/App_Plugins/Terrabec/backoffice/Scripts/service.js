angular.module('umbraco.resources').factory('terrabecService', ['$http', '$q', function ($http, $q) {

	var apiRoot = '/umbraco/backoffice/terrabec/TerrabecApi/';

	function get(url) {
		var deferred = $q.defer();
		$http.get(url).then(function (response) {
			return deferred.resolve(response.data);
		}, function (response) {
			return deferred.reject(response);
		});
		return deferred.promise;
	}

	return {
		refreshConnector: function (id) {
			return get(apiRoot + 'refreshConnector?nodeId=' + id);
		},
		refreshQueue: function (id) {
			return get(apiRoot + 'refreshQueue?nodeId=' + id);
		},
		connectors: function () {
			return get(apiRoot + 'connectors');
		},
		connector: function (id) {
			return get(apiRoot + 'connector?nodeId=' + id);
		},
		connectorInfo: function (id) {
			return get(apiRoot + 'connectorInfo?nodeId=' + id);
		},
		enumerateLists: function (id) {
			return get(apiRoot + 'connectorEnumerateLists?nodeId=' + id);
		},
		readList: function (id) {
			return get(apiRoot + 'ConnectorReadList?nodeId=' + id);
		},
		createList: function (id, name) {
			return get(apiRoot + 'ConnectorCreateList?nodeId=' + id + "&name=" + name);
		},
		queues: function () {
			return get(apiRoot + 'queues');
		},
		queue: function (id) {
			return get(apiRoot + 'queue?nodeId=' + id);
		},
		queueInfo: function (id) {
			return get(apiRoot + 'queueInfo?nodeId=' + id);
		},
		enumerateEmailTemplates: function (id) {
			return get(apiRoot + 'ConnectorEnumerateEmailTemplates?nodeId=' + id);
		},
		readEmailTemplate: function (id) {
			return get(apiRoot + 'ConnectorReadEmailTemplate?nodeId=' + id);
		},
		readSubscriptions: function (id) {
			if (typeof id === 'undefined') {
				id = -1;
			}
			return get(apiRoot + 'SubscriptionEnumerateSubscriptions?nodeId=' + id);
		},
		sendEmail: function (id, email) {
			return get(apiRoot + 'SendEmail?nodeId=' + id + '&email='+email);
		}

	};
}]);