angular.module('umbraco').controller('terrabec.queue',
	[
		'$scope', '$route', '$routeParams', '$q', '$timeout', 'navigationService', 'localizationService', 'terrabecService',
		function ($scope, $route, $routeParams, $q, $timeout, navigationService, localizationService, terrabecService) {
			var vm = this;

			angular.extend(vm, {
				data: {},
				loading: true,
				nodeId: $routeParams.id,
				overview: null,
				info: null,
				refreshButtonText: '',
				init: function () {
					$q.all({
						overview: terrabecService.queue(vm.nodeId),
						info: terrabecService.queueInfo(vm.nodeId),
						translateRefresh: localizationService.localize('TerrabecInfo_RefreshButton')
					}).then(function (response) {
						vm.overview = response.overview;
						vm.info = response.info;
						vm.refreshButtonText = response.translateRefresh;
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: [-1, $routeParams.id], forceReload: false });
							vm.loading = false;
						});
					});
				},
				refresh: function () {
					$q.all({
						refresh: terrabecService.refreshQueue(vm.nodeId)
					}).then(function () {
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: [-1], forceReload: true }).then(function () {
								$route.reload();
							});
							//navigationService.reloadNode($scope.currentNode.parent());
						});
					});
				}

			});
		}
	]
);
