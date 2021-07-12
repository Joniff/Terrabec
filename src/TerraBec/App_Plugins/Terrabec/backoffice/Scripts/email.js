angular.module('umbraco').controller('terrabec.email',
	[
		'$scope', '$routeParams', '$location', '$q', '$timeout', 'navigationService', 'terrabecService',
		function ($scope, $routeParams, $location, $q, $timeout, navigationService, terrabecService) {
			var vm = this;

			angular.extend(vm, {
				loading: true,
				nodeId: $routeParams.id,
				emails: [],
				email: null,
				overview: null,
				info: null,
				emailAddress: null,
				path: [],
				iframe: {},
				htmlDecode: function (input)
				{
					var doc = new DOMParser().parseFromString(input, 'text/html');
					return doc.documentElement.textContent;
				},
				init: function (all) {
					$q.all({
						overview: terrabecService.connector(vm.nodeId),
						emails: all ? terrabecService.enumerateEmailTemplates(vm.nodeId) : terrabecService.readEmailTemplate(vm.nodeId)
					}).then(function (response) {
						vm.overview = response.overview;
						vm.path = [-1, vm.overview.nodeId];
						if (all) {
							vm.emails = response.emails;
						} else {
							vm.path.push(vm.overview.emailTemplatesNodeId);
							vm.email = response.emails;
							vm.iframe = JSON.parse('"' + vm.email.content + '"');
						}
						vm.path.push($routeParams.id);
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: vm.path, forceReload: false });
							vm.loading = false;
						});

					});
				},
				selectEmailTemplate: function (nodeId) {
					$location.path('/' + $routeParams.section + '/' + $routeParams.section + '/emailtemplate/' + nodeId);
				}
			});
		}
	]
);
