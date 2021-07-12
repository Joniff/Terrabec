angular.module('umbraco').controller('terrabec.list',
	[
		'$scope', '$route', '$routeParams', '$location', '$q', '$timeout', 'notificationsService', 'treeService', 'navigationService', 'terrabecService',
		function ($scope, $route, $routeParams, $location, $q, $timeout, notificationsService, treeService, navigationService, terrabecService) {
			var vm = this;

			angular.extend(vm, {
				loading: 0,
				edit: 1,
				saving: 2,
				saved: 3,
				error: 4,
				state: 0,
				nodeId: $routeParams.id,
				lists: [],
				list: null,
				overview: null,
				info: null,
				subscriptions: [],
				listName: null,
				path: [],
				init: function (all) {
					$q.all({
						overview: terrabecService.connector(vm.nodeId),
						lists: (all) ? terrabecService.enumerateLists(vm.nodeId) : terrabecService.readList(vm.nodeId),
					}).then(function (response) {
						vm.overview = response.overview;
						vm.path = [-1, vm.overview.nodeId];
						if (all) {
							vm.lists = response.lists;
						} else {
							vm.list = response.lists;
							vm.path.push(vm.overview.listsNodeId);
						}
						vm.path.push($routeParams.id);
						$q.all({
							subscriptions: terrabecService.readSubscriptions(vm.nodeId)
						}).then(function (response) {
							vm.subscriptions = response.subscriptions;
							$timeout(function () {
								navigationService.syncTree({ tree: $routeParams.section, path: vm.path, forceReload: false });
								vm.state = vm.edit;
							});

							//$scope.$watch('createList.listName', function (newValue, oldValue) {
							//	if (newValue) {
							//		for (var i = 0; i != vm.lists.length; i++) {
							//			if (vm.lists[i].name == newValue) {
							//				$scope.createList.listName.$setValidity('unqiue', false);
							//				return;
							//			}
							//		}
							//		$scope.createList.listName.$setValidity('unqiue', true);
							//	}
							//});
						});

					});
				},
				selectList: function (nodeId) {
					$location.path('/' + $routeParams.section + '/' + $routeParams.section + '/list/' + nodeId);
				},
				createList: function (name) {
					vm.state = vm.saving;
					$q.all({
						list: terrabecService.createList(vm.nodeId, name)
					}).then(function (response) {
						vm.list = response.list;
						vm.state = vm.saved;
					}, function () {
						vm.state = vm.error;
					});
				},
				close: function () {
					navigationService.hideDialog();
					$timeout(function () {
						navigationService.reloadNode($scope.currentNode.parent());
						$route.reload();
					});
				}
			});
		}
	]
);
