angular.module('umbraco').controller('terrabec.overview',
	[
		'$scope', '$route', '$routeParams', '$location', '$q', '$timeout','navigationService', 'terrabecService',
		function ($scope, $route, $routeParams, $location, $q, $timeout, navigationService, terrabecService) {
			var vm = this;

			angular.extend(vm, {
				loading: true,
				refreshing: true,
				connectors: null,
				subscriptions: null,
				init: function () {
					$q.all({
						connectors: terrabecService.connectors(),
						subscriptions: terrabecService.readSubscriptions(),
					}).then(function (response) {
						vm.connectors = response.connectors;
						vm.subscriptions = response.subscriptions;
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: [-1], forceReload: false });
							vm.loading = false;
						});
					});
				},
				selectInfo: function (nodeId) {
					$location.path('/' + $routeParams.section + '/' + $routeParams.section + '/Connector/' + nodeId);
				},
				refresh: function () {
					$q.all({
						refresh: terrabecService.refresh()
					}).then(function () {
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: [-1], forceReload: true }).then(function () {
								$route.reload();
							});
							//navigationService.reloadNode($scope.currentNode.parent());
						});
					});
				},
				close: function () {
					navigationService.hideDialog();
				}
			});
		}
	]
);
