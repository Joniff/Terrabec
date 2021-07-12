angular.module('umbraco').controller('terrabec.property.editor.enumerate.queues',
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
			queues: [],
			selectedQueue: '',
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
				vm.state = vm.loading;

				try {
					if (typeof $scope.model.value !== 'string') {
						$scope.model.value = null;
					}
				}
				catch (oh) {
					$scope.model.value = null;
				}

				$q.all({
					queues: terrabecService.queues(),
					translateNullQueue: localizationService.localize('TerrabecPropertyEditorEnumerateQueues_NullList')
				}).then(function (response) {
					vm.queues.push({
						id: '',
						name: response.translateNullQueue
					});
					angular.forEach(response.queues, function (queue, queueIndex) {
						vm.queues.push(queue);
						if (queue.id === $scope.model.value) {
							vm.selectedQueue = queue.id;
						}
					});
					vm.state = vm.edit;
				}, vm.setErrorState);
			},
			selectedListChange: function (selected) {
				$scope.model.value = selected;
			}
		});
	}]
);

