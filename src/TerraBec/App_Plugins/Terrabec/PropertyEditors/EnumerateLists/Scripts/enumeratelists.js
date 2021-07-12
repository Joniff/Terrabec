angular.module('umbraco').controller('terrabec.property.editor.enumerate.lists',
	['$scope', '$route', '$routeParams', '$location', '$q', '$timeout', 'localizationService', 'terrabecService',
	function ($scope, $route, $routeParams, $location, $q, $timeout, localizationService, terrabecService) {
			var vm = this;

		angular.extend(vm, {
			identifier: $scope.$id + (new Date().getTime()),
			loading: 0,
			edit: 1,
			saving: 2,
			saved: 3,
			error: 4,
			preview: 5,
			state: 0,
			connectors: [],
			lists: [],
			selectedList: '',
			setErrorState: function () {
				vm.state = vm.error;
			},
			initConfig: function () {
				vm.state = vm.edit;
			},
			initEditor: function () {
				if (!angular.isUndefined($scope.model.sortOrder)) {
					vm.state = vm.preview;
					return;
				}

				try {
					if (typeof ($scope.model.value) === 'string') {
						$scope.model.value = ($scope.model.value !== '') ? JSON.parse($scope.model.value) : null;
					}
					if (!$scope.model.value) {
						$scope.model.value = {};
					}
				}
				catch (oh) {
					$scope.model.value = {};
				}

				$q.all({
					connectors: terrabecService.connectors(),
					translateNullList: localizationService.localize('TerrabecPropertyEditorEnumerateLists_NullList')
				}).then(function (response) {
					vm.connectors = response.connectors;
					vm.lists.push({
						id: '',
						name: response.translateNullList
					});

					var promises = [];
					angular.forEach(vm.connectors, function (connector) {
						promises.push((function (nodeId) { return terrabecService.enumerateLists(nodeId); })(connector.nodeId));
					});
					$q.all(promises).then(function (responses) {
						angular.forEach(vm.connectors, function (connector, connectorIndex) {
							connector.lists = responses[connectorIndex];
							angular.forEach(connector.lists, function (list, listIndex) {
								var item = {
									id: connector.id + '/' + list.id,
									name: vm.connectors.length > 1 ? connector.name + ' - ' + list.name : list.name,
									connector: connector.id,
									list: list.id
								};

								if ($scope.model.value.connector === connector.id && $scope.model.value.list === list.id) {
									vm.selectedList = item.id;
								}

								vm.lists.push(item);
							});
						});
						vm.state = vm.edit;
						
					}, vm.setErrorState);
				}, vm.setErrorState);
			},
			selectedListChange: function (selected) {
				angular.forEach(vm.lists, function (list, index) {
					if (list.id === selected) {
						$scope.model.value = {
							connector: list.connector,
							list: list.list
						};
					}
				});
			}
		});
	}]
);

