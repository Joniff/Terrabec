angular.module('umbraco').controller('terrabec.property.editor.enumerate.email.templates',
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
			emailTemplates: [],
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
						$scope.model.value = ($scope.model.value != '') ? JSON.parse($scope.model.value) : null;
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
					translateNullList: localizationService.localize('TerrabecPropertyEditorEnumerateEmailTemplates_NullList')
				}).then(function (response) {
					vm.connectors = response.connectors;
					vm.emailTemplates.push({
						id: '',
						name: response.translateNullList
					});

					var promises = [];
					angular.forEach(vm.connectors, function (connector) {
						promises.push((function (nodeId) { return terrabecService.enumerateEmailTemplates(nodeId) })(connector.nodeId));
					});
					$q.all(promises).then(function (responses) {
						angular.forEach(vm.connectors, function (connector, connectorIndex) {
							connector.emailTemplates = responses[connectorIndex];
							angular.forEach(connector.emailTemplates, function (emailTemplate, emailTemplateIndex) {
								var item = {
									id: connector.id + '/' + emailTemplate.id,
									name: vm.connectors.length > 1 ? connector.name + ' - ' + emailTemplate.name : emailTemplate.name,
									connector: connector.id,
									emailTemplate: emailTemplate.id
								};

								if ($scope.model.value.connector == connector.id && $scope.model.value.emailTemplate == emailTemplate.id) {
									vm.selectedList = item.id;
								}

								vm.emailTemplates.push(item);
							});
						});
						vm.state = vm.edit;
						
					}, vm.setErrorState);
				}, vm.setErrorState);
			},
			selectedEmailTemplateChange: function (selected) {
				angular.forEach(vm.emailTemplates, function (emailTemplate, index) {
					if (emailTemplate.id == selected) {
						$scope.model.value = {
							connector: emailTemplate.connector,
							emailTemplate: emailTemplate.emailTemplate
						};
					}
				});
			}
		});
	}]
);

